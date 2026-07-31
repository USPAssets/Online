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

void ImportFontGmx(UndertaleFont ufont, string fontGmxPath) {
	XmlDocument xdoc = new XmlDocument();
	xdoc.Load(fontGmxPath);
	
	int gfirst = int.MaxValue /* def: 32 */;
	int glast = int.MinValue /* def: 127 */;
	
	List<Tuple<int/*:mychar*/, int/*:other*/, int/*:amount*/>> kpairslist = new List<Tuple<int/*:mychar*/, int/*:other*/, int/*:amount*/>>();
	
	foreach (XmlNode xnode in xdoc.SelectNodes("/font/*")) {
		string xname = xnode.Name;
		switch (xname) {
			default: {
				ScriptError($"Unknown entry '{xname}' found in {fontGmxPath}.", "GMX Error");
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
				// ignore, will be imported by graphics importer
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
}

void ImportAllFontGlyphs(string fontsPath) {
	foreach (string file in Directory.EnumerateFiles(fontsPath, "*.font.gmx", SearchOption.TopDirectoryOnly)) {
		// Strip both .font and .gmx extensions
		string fontName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file));
		UndertaleFont? font = Data.Fonts.ByName(fontName);
		if (font == null) {
			continue;
		}
		ImportFontGmx(font, file);
	}
}