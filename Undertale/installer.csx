using System;
using System.Threading.Tasks;

using UndertaleModLib;
using UndertaleModLib.Scripting;
using System.IO;
using System.Reflection;
using UndertaleModLib.Util;
using System.Xml.Linq;
using UndertaleModLib.Models;
using System.Collections.Generic;
using System.Linq;


enum StrType : int
{
	NOT_UT_ERROR,
	NO_ASSET_ERROR,
	WANT_TRANSLATE,
	FMT_AN_ERROR,
	DONE,
	ERROR_TITLE,
	SPRITES,
	FONTS,
	SOUNDS,
	STRINGS,
	STANDALONE,
	YOURE_UPDATING,
	NO_NEED_TO,
	SPLASH,
	VERSION_ERR,
	TOONEW_ERR,
	_LENGTH
}

// -- translate this please -- //
string[] stringArray = new string[(int)StrType._LENGTH];
string RealScriptPath = Path.GetDirectoryName(ScriptPath);

async Task ExecuteTranslation(string path)
{
	EnsureDataLoaded(); // so we only work with actual files.
	stringArray[(int)StrType.NOT_UT_ERROR] = "Il gioco che vuoi tradurre non é UNDERTALE...\r\n Sei sicuro di stare bene :(?";
	stringArray[(int)StrType.NO_ASSET_ERROR] = "Mancano alcune risorse necessarie per la traduzione!\r\n Perfavore riscarica USP.";
	stringArray[(int)StrType.WANT_TRANSLATE] = "Adesso il gioco verrá'tradotto\r\n";
	stringArray[(int)StrType.FMT_AN_ERROR] = "Durante l'esecuzione é stato lanciato il seguente errore {0}:\r\n{1}";
	stringArray[(int)StrType.DONE] = "Finito!\r\nLa traduzione dovrebbe essere stata applicata correttamente.";
	stringArray[(int)StrType.SPRITES] = "sprites";
	stringArray[(int)StrType.FONTS] = "fonts";
	stringArray[(int)StrType.SOUNDS] = "sounds";
	stringArray[(int)StrType.STRINGS] = "strings";
	stringArray[(int)StrType.ERROR_TITLE] = "Error";
	stringArray[(int)StrType.STANDALONE] = "Questo script deve essere eseguito come un unico e solo .csx file!";
	stringArray[(int)StrType.YOURE_UPDATING] = "Gioco gia' tradotto, provo ad aggiornare la traduzione...";
	stringArray[(int)StrType.SPLASH] = "splash";
	stringArray[(int)StrType.NO_NEED_TO] = "Non c'e' bisogno di aggiornare la versione {0}.";
	stringArray[(int)StrType.VERSION_ERR] = "Un downgrade dalla versione {0} alla versione {1} non e' possibile.";
	stringArray[(int)StrType.TOONEW_ERR] = "La versione della traduzione e' troppo nuova o sconosciuta. Scarica un installer piu' recente o reinstalla il gioco";

	//if (RealScriptPath is null) {
	//	ScriptError(stringArray[(int)StrType.STANDALONE], stringArray[(int)StrType.ERROR_TITLE], true);
	//	return;
	//}

	
	bool isUndertale = (Data.GeneralInfo.DisplayName.Content.ToLower() == "undertale" || Data.GeneralInfo.DisplayName.Content.ToLower() == "nxtale");
	// i don't think nxtale will work but idfk lol.
	// UPD: it works... sorta...

	if (!isUndertale)
	{
		throw new Exception(stringArray[(int)StrType.NOT_UT_ERROR]);
	}

	if (!Directory.Exists(path))
	{
		throw new Exception(stringArray[(int)StrType.NO_ASSET_ERROR]);
	}

	UpdateProgress("Sounds...");
	await Task.Yield(); // let UI update.
	ImportSounds();

	UpdateProgress("Sprites (ci metterà un po' di tempo)...");
	await Task.Yield(); // let UI update.
	await ImportSprites();

	UpdateProgress("Fonts...");
	await Task.Yield(); // let UI update.
	ImportFonts();

	UpdateProgress("Strings...");
	await Task.Yield(); // let UI update.
	ImportStrings();

	UpdateProgress("Splash screen...");
	await Task.Yield(); // let UI update.
	ImportSplash();

	UpdateProgress("Concludo il processo...");
	await Task.Yield(); // let UI update.
	Finish();
}

int g_NumSteps = 5;
int g_CurrentStep = 0;

void UpdateProgress(string n)
{
	UpdateProgressBar(
		n,
		"...",
		g_CurrentStep++, g_NumSteps
	);
}

// --- Import Sounds --- //
void ImportSounds()
{
	string path = Path.Combine(RealScriptPath, "Sounds");
	string winpath = Path.GetDirectoryName(FilePath);

	try
	{
		// Import flowey sound.
		byte[] wonderfulidea = File.ReadAllBytes(Path.Combine(path, "snd_wonderfulidea.wav"));
		var snd = Data.Sounds.ByName("snd_wonderfulidea");
		if (snd is null)
		{
			// sound... does not exist?!
			throw new Exception("Sound file does not exist.");
		}

		snd.AudioFile.Data = wonderfulidea;

		// Copy Mettaton sound.
		File.Copy(Path.Combine(path, "mus_ohyes.ogg"), Path.Combine(winpath, "mus_ohyes.ogg"), true);
	}
	catch (Exception e)
	{
		throw new Exception(string.Format(stringArray[(int)StrType.FMT_AN_ERROR], stringArray[(int)StrType.SOUNDS], e.ToString()));
	}
}

// --- Helper for special sprites --- //
void handleSpecialSprite(UndertaleSprite sprite)
{
	var tex = sprite.Textures[0].Texture;
	var name = sprite.Name.Content;
	switch (name)
	{
		case "spr_cbone":
			{
				tex.SourceWidth -= 1;
				tex.TargetWidth -= 1;
				break;
			}

		case "spr_oolbone":
			{
				tex.SourceWidth -= 15;
				tex.TargetWidth -= 15;
				break;
			}

		// spr_dbone surprisingly has the same w/h so it's not touched here at all.

		case "spr_udebone":
			{
				tex.SourceWidth -= 4;
				tex.TargetWidth -= 4;
				break;
			}

		case "spr_bulletNapstaSad":
			{
				tex.SourceWidth -= 3;
				tex.TargetWidth -= 3;
				break;
			}

		default:
			{
				break;
			}
	}
}

async Task ImportSprites()
{
	string path = Path.Combine(RealScriptPath, "Sprites");
	
	try
	{
		foreach (var spr in Data.Sprites)
		{
			if (spr is null) continue;

			string name = spr.Name.Content;

			// if sprite needs additional fixes.
			handleSpecialSprite(spr);

			for (int t = 0; t < spr.Textures.Count; t++)
			{
				string fullname = Path.Combine(path, name + "_" + t.ToString() + ".png");
				if (!File.Exists(fullname))
				{
					continue;
				}

				var tex = TextureWorker.ReadBGRAImageFromFile(fullname);
				spr.Textures[t].Texture.ReplaceTexture(tex);
			}
			await Task.Yield(); // let UI update.
		}
	}
	catch (Exception e)
	{
		throw new Exception(string.Format(stringArray[(int)StrType.FMT_AN_ERROR], stringArray[(int)StrType.SPRITES], e.ToString()));
	}
}

void ImportFonts()
{
	string path = Path.Combine(RealScriptPath, "Fonts");
	try	{
		// -- Importing Glyph & Tex Data -- //

		string glyphdatapath = Path.Combine(path, "glyphdata.xml");
		var glyphdata = XDocument.Load(glyphdatapath);
		foreach (var xel in glyphdata.Root.Elements())
		{
			var fnt = Data.Fonts.ByName(xel.Name.ToString());
			fnt.Glyphs.Clear();
			foreach (var glyph in xel.Elements("Glyph"))
			{
				var character = int.Parse(glyph.Attribute("Character").Value);
				var x = int.Parse(glyph.Attribute("SourceX").Value);
				var y = int.Parse(glyph.Attribute("SourceY").Value);
				var w = int.Parse(glyph.Attribute("SourceWidth").Value);
				var h = int.Parse(glyph.Attribute("SourceHeight").Value);
				var shift = int.Parse(glyph.Attribute("Shift").Value);
				var offset = int.Parse(glyph.Attribute("Offset").Value);

				fnt.Glyphs.Add(new UndertaleFont.Glyph
				{
					Character = (ushort)character,
					SourceX = (ushort)x,
					SourceY = (ushort)y,
					SourceWidth = (ushort)w,
					SourceHeight = (ushort)h,
					Shift = (short)shift,
					Offset = (short)offset
				});

				fnt.RangeEnd = (uint)character;
			}

			var tgitem = xel.Element("TGItem");
			fnt.Texture.SourceX = ushort.Parse(tgitem.Attribute("SourceX").Value);
			fnt.Texture.SourceY = ushort.Parse(tgitem.Attribute("SourceY").Value);
			fnt.Texture.SourceWidth = ushort.Parse(tgitem.Attribute("SourceWidth").Value);
			fnt.Texture.SourceHeight = ushort.Parse(tgitem.Attribute("SourceHeight").Value);

			fnt.Texture.TargetX = ushort.Parse(tgitem.Attribute("TargetX").Value);
			fnt.Texture.TargetY = ushort.Parse(tgitem.Attribute("TargetY").Value);
			fnt.Texture.TargetWidth = ushort.Parse(tgitem.Attribute("TargetWidth").Value);
			fnt.Texture.TargetHeight = ushort.Parse(tgitem.Attribute("TargetHeight").Value);

			fnt.Texture.BoundingWidth = ushort.Parse(tgitem.Attribute("BoundingWidth").Value);
			fnt.Texture.BoundingHeight = ushort.Parse(tgitem.Attribute("BoundingHeight").Value);
		}

		// -- Importing Font Texture Blobs -- //
		var tpage1 = Data.Fonts.ByName("fnt_main").Texture;
		var tpage2 = Data.Fonts.ByName("fnt_wingdings").Texture;
		string fonttexpath1 = Path.Combine(path, "fonttex1.png");
		string fonttexpath2 = Path.Combine(path, "fonttex2.png");

		tpage1.TexturePage.TextureData.Image = GMImage.FromPng(File.ReadAllBytes(fonttexpath1))
                                          .ConvertToFormat(tpage1.TexturePage.TextureData.Image.Format);

		//tpage2.TexturePage.TextureData.TextureBlob = File.ReadAllBytes(fonttexpath2);
		// ^ behaves weird on PSVita when replacing an entire page...
		// ^^ button sprites get messed up... (perhaps different positions??????)
		//8-4 Ltd what are you doing

		// there we go, an alternative method to inject gaster font.
		var wingding = TextureWorker.ReadBGRAImageFromFile(fonttexpath2);
		tpage2.ReplaceTexture(wingding);


		// -- Fixing four little sprites after that -- //
		var spr1 = Data.Sprites.ByName("button_ps4_dpad_l").Textures[0].Texture;
		var spr2 = Data.Sprites.ByName("button_ps4_dpad_r").Textures[0].Texture;
		var spr3 = Data.Sprites.ByName("button_vita_dpad_l").Textures[0].Texture;
		var spr4 = Data.Sprites.ByName("button_vita_dpad_r").Textures[0].Texture;

		spr1.SourceX = (ushort)2;
		spr1.SourceY = (ushort)910;
		spr1.SourceWidth = (ushort)12;
		spr1.SourceHeight = (ushort)7;
		spr1.TargetX = (ushort)0;
		spr1.TargetY = (ushort)4;
		spr1.TargetWidth = (ushort)12;
		spr1.TargetHeight = (ushort)7;

		spr3.SourceX = (ushort)2;
		spr3.SourceY = (ushort)910;
		spr3.SourceWidth = (ushort)12;
		spr3.SourceHeight = (ushort)7;
		spr3.TargetX = (ushort)0;
		spr3.TargetY = (ushort)4;
		spr3.TargetWidth = (ushort)12;
		spr3.TargetHeight = (ushort)7;

		spr2.SourceX = (ushort)522;
		spr2.SourceY = (ushort)782;
		spr2.SourceWidth = (ushort)12;
		spr2.SourceHeight = (ushort)7;
		spr2.TargetX = (ushort)0;
		spr2.TargetY = (ushort)4;
		spr2.TargetWidth = (ushort)12;
		spr2.TargetHeight = (ushort)7;

		spr4.SourceX = (ushort)522;
		spr4.SourceY = (ushort)782;
		spr4.SourceWidth = (ushort)12;
		spr4.SourceHeight = (ushort)7;
		spr4.TargetX = (ushort)0;
		spr4.TargetY = (ushort)4;
		spr4.TargetWidth = (ushort)12;
		spr4.TargetHeight = (ushort)7;
		// no need to change BoundingWidth/Height.

	}
	catch (Exception e)	{
		throw new Exception(string.Format(stringArray[(int)StrType.FMT_AN_ERROR], stringArray[(int)StrType.FONTS], e.ToString()));
	}
}

void ImportStrings()
{
	var lookupOriginalPath = Path.Combine(RealScriptPath, "Strings", "original.txt");
	var lookupTranslatedPath = Path.Combine(RealScriptPath, "Strings", "translated.txt");
	var lookupDict = new Dictionary<string, string>();
    if (!File.Exists(lookupOriginalPath) || !File.Exists(lookupTranslatedPath)) {
        ScriptError("Lookup files do not exist, strings will not be imported.", "Strings Import Error");
        return;
    }

    using (StreamReader orig = new StreamReader(lookupOriginalPath)) {
        using (StreamReader transl = new StreamReader(lookupTranslatedPath)) {
            string? line;
            while ((line = orig.ReadLine()) != null) {
                lookupDict[line] = transl.ReadLine() ?? throw new Exception("Translation file has less lines than expected.");
            }
        }
    }
    
    foreach (var str in Data.Strings) {
        if (str.Content.Contains("\n") || str.Content.Contains("\r"))
            continue;
        if (lookupDict.TryGetValue(str.Content, out var translation)) {
            str.Content = translation;
        }
    }
}

void ImportSplash()
{

	string path = Path.Combine(RealScriptPath, "Sprites");
	string winpath = Path.GetDirectoryName(FilePath);

	try
	{
		string utsplashPath = Path.Combine(winpath, "splash.png");
		// the file is already 640x480 (right resolution for game window!)
		string ITsplashPath = Path.Combine(path, "OPTIONAL_splash.png");
		if (File.Exists(utsplashPath) && File.Exists(ITsplashPath)) {
			File.Copy(ITsplashPath, utsplashPath, true);
		}
		// that should be it...
	}
	catch (Exception e)
	{
		// silently ignore any errors since it's not important.
	}
}

// --- Done, hooray! --- //
void Finish()
{
	ScriptMessage(stringArray[(int)StrType.DONE]);
}

await ExecuteTranslation(RealScriptPath);
