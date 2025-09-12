// Smart font import script for Deltarune
// By USP (Nik)

using UndertaleModLib.Util; // TextureWorker
using System.Text; // StringBuilder
using System.Xml; // XmlDocument
using System.Threading.Tasks;
using System.Text.RegularExpressions; // Regex
using System.Linq;
using System.IO;
using ImageMagick;

/** GMX parser stuff */

bool GmxBool(string innerText) {
	int result = 0;
	if (!int.TryParse(innerText, out result)) {
		try {
			result = Convert.ToBoolean(innerText) ? -1 : 0;
		}
		catch {
			ScriptError(string.Format("Failed to parse GMX value '{0}' as a bool.", innerText), "GMX Error");
		}
	}
	
	return result != 0;
}

int GmxInt(string innerText) {
	int result = 0;
	if (!int.TryParse(innerText, out result)) {
		// TODO: exception?
		ScriptError(string.Format("Failed to parse GMX value '{0}' as an int.", innerText), "GMX Error");
	}
	
	return result;
}

string GmxString(string innerText) {
	// TODO: sanitize string from XML stuff
	if (innerText is null) {
		ScriptError(string.Format("The GMX string '{0}' is null.", innerText), "GMX Error");
	}
	
	return innerText;
}

void SmartFontReplace(string fontsPath, string origName, string scapegoatName) {

	string fontGmx = Path.Combine(fontsPath, origName + ".font.gmx");
	string fontPng = null;
	
	UndertaleFont ufont = Data.Fonts.ByName(origName);
	if (ufont is null) {
		ScriptError(string.Format("Font '{0}' was not found in the game.", origName), "Font Error");
		return;
	}
	
	XmlDocument xdoc = new XmlDocument();
	xdoc.Load(fontGmx);
	
	int gfirst = int.MaxValue /* def: 32 */;
	int glast = int.MinValue /* def: 127 */;
	
	List<Tuple<int/*:mychar*/, int/*:other*/, int/*:amount*/>> kpairslist = new List<Tuple<int/*:mychar*/, int/*:other*/, int/*:amount*/>>();
	
	foreach (XmlNode xnode in xdoc.SelectNodes("/font/*")) {
		string xname = xnode.Name;
		switch (xname) {
			default: {
				ScriptError(string.Format("Unknown entry '{0}' found in Font GMX.", xname), "GMX Error");
				return;
			}
			
			case "name": {
				string gmxfontname = GmxString(xnode.InnerText);
				ufont.DisplayName.Content = gmxfontname;
				break;
			}
			
			case "renderhq": {
				// this is actually unused but still parsed to prevent errors.
				bool buserenderhq = GmxBool(xnode.InnerText);
				break;
			}
			
			case "includeTTF": {
				// this *may* be useful if the script will render all textures in standalone.
				bool bincludettf = GmxBool(xnode.InnerText);
				break;
			}
			
			case "TTFName": {
				// see includeTTF, if includeTTF is true, usually this is always set.
				string bttffilepath = GmxString(xnode.InnerText);
				break;
			}
			
			case "texgroups": {
				// just parse them but not actually do anything.
				// since texgroups are already handled by the game.
				foreach (XmlNode xtexgroupnode in xnode.ChildNodes) {
					string texstring = GmxString(xtexgroupnode.Name).Substring("texgroup".Length);
					int texindex = GmxInt(texstring);
					int texassetid = GmxInt(xtexgroupnode.InnerText);
					// TextureGroups[texindex] = texassetid;
				}
				break;
			}
			
			case "kerningPairs": {
				foreach (XmlNode xkern in xnode.SelectNodes("pair")) {
					int knum = GmxInt(xkern.Attributes["first"].Value);
					int kother = GmxInt(xkern.Attributes["second"].Value);
					int kamount = GmxInt(xkern.Attributes["amount"].Value);
					kpairslist.Add(new Tuple<int/*:mychar*/, int/*:other*/, int/*:amount*/>(knum/*mychar*/, kother/*other*/, kamount/*amount*/));
				}
				break;
			}
			
			case "italic": {
				ufont.Italic = GmxBool(xnode.InnerText);
				break;
			}
			
			case "bold": {
				ufont.Bold = GmxBool(xnode.InnerText);
				break;
			}
			
			case "size": {
				ufont.EmSize = checked((uint)GmxInt(xnode.InnerText));
				break;
			}
			
			case "charset": {
				ufont.Charset = checked((byte)GmxInt(xnode.InnerText));
				break;
			}
			
			case "aa": {
				ufont.AntiAliasing = checked((byte)GmxInt(xnode.InnerText));
				break;
			}
			
			case "image": {
				fontPng = Path.Combine(fontsPath, GmxString(xnode.InnerText));
				break;
			}
			
			case "ranges": {
				foreach (XmlNode xrange in xnode.ChildNodes) {
					string[] rarr = GmxString(xrange.InnerText).Split(',');
					int ra = GmxInt(rarr[0/*first*/]);
					int rb = GmxInt(rarr[1/*last*/]);
					gfirst = Math.Min(gfirst, ra);
					glast = Math.Max(glast, rb);
				}
				break;
			}
			
			case "glyphs": {
				ufont.Glyphs.Clear();
				foreach (XmlNode xglyph in xnode.SelectNodes("glyph")) {
					int gchar = GmxInt(xglyph.Attributes["character"].Value);
					int gx = GmxInt(xglyph.Attributes["x"].Value);
					int gy = GmxInt(xglyph.Attributes["y"].Value);
					int gw = GmxInt(xglyph.Attributes["w"].Value);
					int gh = GmxInt(xglyph.Attributes["h"].Value);
					int gshift = GmxInt(xglyph.Attributes["shift"].Value);
					int goffset = GmxInt(xglyph.Attributes["offset"].Value);
					ufont.Glyphs.Add(new UndertaleFont.Glyph() {
						Character = checked((ushort)gchar),
						SourceX = checked((ushort)gx),
						SourceY = checked((ushort)gy),
						SourceWidth = checked((ushort)gw),
						SourceHeight = checked((ushort)gh),
						Shift = checked((short)gshift),
						Offset = checked((short)goffset),
						Kerning = new UndertaleSimpleListShort<UndertaleFont.Glyph.GlyphKerning>()
					});
				}
				
				break;
			}
		}
	}
	
	// post process...
	ufont.RangeStart = checked((ushort)gfirst);
	ufont.RangeEnd = checked((ushort)glast);

    // This is broken, and also not needed. Ignore for now.
	// foreach (var ktuple in kpairslist) {
	// 	foreach (var theglyph in ufont.Glyphs) {
	// 		if (ktuple.Item1/*:mychar*/ == theglyph.Character) {
	// 			// found our kerning pair:
	// 			theglyph.Kerning.Add(new UndertaleFont.Glyph.GlyphKerning() {
	// 				Other = checked((short)ktuple.Item2/*:other*/),
	// 				Amount = checked((short)ktuple.Item3/*:amount*/)
	// 			});
	// 		}
	// 	}
	// }
	
	// obtain font texture
	var myFontTexture = TextureWorker.ReadBGRAImageFromFile(fontPng);
	
	var targetFontTexture = ufont.Texture;
	
	var doScapegoat = (!(scapegoatName is null)) && scapegoatName.Length > 0;
	if (doScapegoat) {
		ufont.Texture = Data.Fonts.ByName(scapegoatName).Texture;
		targetFontTexture = ufont.Texture;
	}
	
	// ensure that we're not going to horribly break shit.
	if (myFontTexture.Width  > targetFontTexture.SourceWidth
	||  myFontTexture.Height > targetFontTexture.SourceHeight) {
		ScriptError("The font texture is LARGER than the game's.", "Font Import Error");
	}
	
	// this will shrink down the texture if using scapegoat font
	// or if the font happens to be larger.
	targetFontTexture.SourceWidth = checked((ushort)myFontTexture.Width);
	targetFontTexture.SourceHeight = checked((ushort)myFontTexture.Height);
	targetFontTexture.TargetWidth = checked((ushort)myFontTexture.Width);
	targetFontTexture.TargetHeight = checked((ushort)myFontTexture.Height);
	targetFontTexture.BoundingWidth = checked((ushort)myFontTexture.Width);
	targetFontTexture.BoundingHeight = checked((ushort)myFontTexture.Height);
	
	// render the texture on top of the original one.
	targetFontTexture.ReplaceTexture(myFontTexture);
	
	// should be done?
}

void ImportAllFonts(string fontsPath, bool includeTinyNoelle = true) {
	// null - the italian texture is equal to, or smaller.
	// otherwise, a japanese font is specified to abuse it's large AF texture.
	// (the JP texture will be wiped out, and italian font will be drawn at 0;0)
	if (includeTinyNoelle) {
		SmartFontReplace(fontsPath, "fnt_tinynoelle", null);
	}
	SmartFontReplace(fontsPath, "fnt_dotumche", "fnt_ja_dotumche"); // doesn't seem to fit... :(
	// okay now we HAVE to import this font.
	SmartFontReplace(fontsPath, "fnt_mainbig", null);
	SmartFontReplace(fontsPath, "fnt_main", null);
	SmartFontReplace(fontsPath, "fnt_comicsans", null);
	SmartFontReplace(fontsPath, "fnt_small", null);
}