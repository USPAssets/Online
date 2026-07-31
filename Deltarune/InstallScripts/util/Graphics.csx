// Graphics.csx
// By USP

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Linq;
using System.Text;
using UndertaleModLib.Util;
using ImageMagick;

public record SpriteTexture(UndertaleTexturePageItem PageItem, string TextureFile, int Width, int Height);

public void PackSprites(List<SpriteTexture> sprites, bool trimBorder = true, uint maxSize = 2048)
{
    if (sprites.Count == 0) {
        return;
    }

    // Sort by height descending
    sprites.Sort((x, y) => y.Height.CompareTo(x.Height));
    
    int numAtlases = 0;
    MagickImage currentAtlas = new(MagickColors.Transparent, maxSize, maxSize);
    UndertaleEmbeddedTexture currentTexture = new()
    {
        Name = new UndertaleString($"USPTexture {Data.EmbeddedTextures.Count}")
    };

    uint currentX = 0;
    uint currentY = 0;
    uint currentShelfHeight = 0;
    const uint padding = 2;

    foreach (SpriteTexture texture in sprites)
    {
        using MagickImage image = new MagickImage(texture.TextureFile);
        // Use the image's border to determine how to place it in the boundaries
        IMagickGeometry? bbox = null;
        // Then trim it so it fits snugly in the atlas without padding
        if (trimBorder)
        {
            image.BorderColor = MagickColors.Transparent;
            image.BackgroundColor = MagickColors.Transparent;
            // Without adding a transparent border, Trim() will also cut black parts of the image that touch the edges.
            image.Border(1);
            bbox = image.BoundingBox;
            image.Trim();
        }

        if (image.Width > maxSize || image.Height > maxSize)
        {
            throw new ScriptException($"Image {texture.TextureFile} is too big ({image.Width}x{image.Height}). Please increase the max atlas size!");
        }
        if (currentX + image.Width > maxSize)
        {
            // If the rectangle exceeds the bin width, wrap to the next shelf
            currentX = 0;
            currentY += currentShelfHeight;
            currentShelfHeight = 0;
        }
        if (currentY + image.Height > maxSize)
        {
            // If the rectangle exceeds the bin height, we need a new atlas
            currentTexture.TextureData.Image = GMImage.FromMagickImage(currentAtlas).ConvertToPng();
            Data.EmbeddedTextures.Add(currentTexture);

            currentAtlas.Dispose();
            currentAtlas = new MagickImage(MagickColors.Transparent, maxSize, maxSize);
            numAtlases++;
            currentTexture = new UndertaleEmbeddedTexture()
            {
                Name = new UndertaleString($"USPTexture {Data.EmbeddedTextures.Count}")
            };

            currentX = 0;
            currentY = 0;
            currentShelfHeight = 0;
        }

        currentAtlas.Composite(image, (int)currentX, (int)currentY, CompositeOperator.Copy);
        texture.PageItem.SourceX = (ushort)currentX;
        texture.PageItem.SourceY = (ushort)currentY;
        texture.PageItem.SourceWidth = (ushort)image.Width;
        texture.PageItem.SourceHeight = (ushort)image.Height;
        texture.PageItem.TargetWidth = (ushort)image.Width;
        texture.PageItem.TargetHeight = (ushort)image.Height;
        if (trimBorder) {
            // -1 because of the border we added above
            texture.PageItem.TargetX = (ushort)(bbox.X - 1);
            texture.PageItem.TargetY = (ushort)(bbox.Y - 1);
        } else {
            texture.PageItem.TargetX = 0;
            texture.PageItem.TargetY = 0;
        }
        texture.PageItem.BoundingWidth = (ushort)texture.Width;
        texture.PageItem.BoundingHeight = (ushort)texture.Height;
        texture.PageItem.TexturePage = currentTexture;

        currentX += image.Width + padding;
        // The height of the shelf is dictated by the tallest rectangle in it
        currentShelfHeight = Math.Max(currentShelfHeight, image.Height + padding);
    }

    currentTexture.TextureData.Image = GMImage.FromMagickImage(currentAtlas).ConvertToPng();
    Data.EmbeddedTextures.Add(currentTexture);
    currentAtlas.Dispose();
}

static readonly FrozenSet<string> SPRITE_SIZE_EXCEPTIONS = new[] {
    "spr_battlemsg",
    "spr_battlemsg_ch1",
    "spr_funnytext_alligator",
    "spr_funnytext_brother",
    "spr_funnytext_coffee",
    "spr_funnytext_fun_loop",
    "spr_funnytext_know_tv",
    "spr_funnytext_rock_concert",
    "spr_funnytext_tan",
    "spr_conbini_sign_clopen",
}.ToFrozenSet();

void ImportAllSprites(string folder, string suffix = null)
{
    List<SpriteTexture> toImport = new();
    foreach (string file in Directory.EnumerateFiles(folder, "*.png", SearchOption.TopDirectoryOnly)) {
        string fileName = Path.GetFileNameWithoutExtension(file);
        int frameNumIndex = fileName.LastIndexOf('_');
        if (frameNumIndex == -1) {
            throw new Exception($"File {file} does not have a frame number in its name.");
        }
        string spriteName = fileName.Substring(0, frameNumIndex);
        int frameNum = int.Parse(fileName.Substring(frameNumIndex + 1));
        if (suffix != null)
        {
            spriteName += suffix;
            // Try sprite with suffix only if it there's not a separate file for it
            if (File.Exists(Path.Combine(folder, spriteName + '_' + frameNum + ".png")))
            {
                continue;
            }
        }
        UndertaleSprite? sprite = Data.Sprites.ByName(spriteName);
        if (sprite == null) {
            continue;
        }
        if (frameNum >= sprite.Textures.Count) {
            continue;
        }
        (int width, int height) = TextureWorker.GetImageSizeFromFile(file);
        if (width == -1 || height == -1) {
            throw new Exception($"File {file} is not a valid image.");
        }
        if (sprite.Width != width || sprite.Height != height) {
            if (SPRITE_SIZE_EXCEPTIONS.Contains(spriteName)) {
                sprite.Width = (uint)width;
                sprite.Height = (uint)height;
            } else {
                ScriptError($"""
                {spriteName}_{frameNum} has a different size than original ({width}x{height} vs {sprite.Width}x{sprite.Height}).
                    This will cause the sprite to be misaligned. Resize it to fit the original sprite,
                    or add an exception to SPRITE_SIZE_EXCEPTIONS in Graphics.csx if it's an intended change.
                """);
            }
        }
        UndertaleTexturePageItem texture = sprite.Textures[frameNum].Texture;
        toImport.Add(new SpriteTexture(texture, file, width, height));
    }
    PackSprites(toImport);
}

void ImportAllTilesets(string folder)
{
    List<SpriteTexture> toImport = new();
    foreach (string file in Directory.EnumerateFiles(folder, "*.png", SearchOption.TopDirectoryOnly))
    {
        string bgName = Path.GetFileNameWithoutExtension(file);
        UndertaleBackground? bg = Data.Backgrounds.ByName(bgName);
        if (bg == null)
        {
            continue;
        }
        (int width, int height) = TextureWorker.GetImageSizeFromFile(file);
        if (width == -1 || height == -1)
        {
            throw new Exception($"File {file} is not a valid image.");
        }
        UndertaleTexturePageItem texture = bg.Texture;
        if (texture.BoundingWidth != width || texture.BoundingHeight != height)
        {
            Console.WriteLine($"{bgName} has a different size than original ({width}x{height} vs {texture.BoundingWidth}x{texture.BoundingHeight})\nThis will cause the tileset to be misaligned.");
        }
        toImport.Add(new SpriteTexture(texture, file, width, height));
    }

    PackSprites(toImport, false);
}

void ImportAllFontGraphics(string folder)
{
    List<SpriteTexture> toImport = new();
    foreach (string file in Directory.EnumerateFiles(folder, "*.png", SearchOption.TopDirectoryOnly))
    {
        string bgName = Path.GetFileNameWithoutExtension(file);
        UndertaleFont? font = Data.Fonts.ByName(bgName);
        if (font == null)
        {
            continue;
        }
        (int width, int height) = TextureWorker.GetImageSizeFromFile(file);
        if (width == -1 || height == -1)
        {
            throw new Exception($"File {file} is not a valid image.");
        }
        toImport.Add(new SpriteTexture(font.Texture, file, width, height));
    }

    // Fonts are smaller and there's fewer of them, we can use a smaller size
    PackSprites(toImport, false, 512);
}