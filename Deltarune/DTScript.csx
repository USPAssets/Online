/**
 *  Undertale Spaghetti Project
 *  DELTARUNE Chapter 1&2 Translation script
 *
 *  @version 1.4
 *  @author USP
 */

/** usings */
using UndertaleModLib.Util; // TextureWorker
using System.Text; // StringBuilder
using System.Xml; // XmlDocument
using System.Text.RegularExpressions; // Regex
using System.Linq;

/** global vars */

/**
 * -1 - error/undefined
 *  0 - no translation (english)
 *  1 - translation V1
 *  2 - translation V2
 *  3 - translation V3
 *  4 - translation V4
 *  5 and onward - reserved.
 */
int translationVersion = -1;
int myVersion = 5; // this installer is V1.
static Guid V1_GUID = Guid.Parse("{2E50E32D-4932-4DA7-8CB9-B1C3BE410C2C}");
static Guid V2_GUID = Guid.Parse("{7E9EF6F9-1C47-4D3A-A369-E2E375A6EF66}");
static Guid V3_GUID = Guid.Parse("{52E75B10-B0D6-400B-8E0C-41C2CA7CB70A}");
static Guid V4_GUID = Guid.Parse("{8F3D27AB-FD9C-4BFC-BD26-28FEA95A5F05}");
static Guid V5_GUID = Guid.Parse("{BD476F48-9D1F-48BF-BB6E-0732ED6AA440}"); // reserved
static Guid V6_GUID = Guid.Parse("{F1243771-9450-451D-AB3F-4240E440580D}"); // reserved
static Guid V7_GUID = Guid.Parse("{B46E1A9D-645B-4EA1-B067-9C60EED1608F}"); // reserved
static Guid V8_GUID = Guid.Parse("{23406B09-2BAE-45B6-85AA-C67F73B559FF}"); // reserved
static Guid V9_GUID = Guid.Parse("{63B6F4FF-2D25-48E6-8775-44823C8F3AC6}"); // reserved

int DetermineTranslationVersion(UndertaleData dd)
{
	Guid myguid = dd.GeneralInfo.DirectPlayGuid;
	if (myguid == Guid.Empty)
		return 0; // english..?
	else if (myguid == V1_GUID)
		return 1; // v1
	else if (myguid == V2_GUID)
		return 2; // v2
	else if (myguid == V3_GUID)
		return 3; // v3
	else if (myguid == V4_GUID)
		return 4; // v4
	else if (myguid == V5_GUID)
		return 5; // v5 reserved
	else if (myguid == V6_GUID)
		return 6; // v6 reserved
	else if (myguid == V7_GUID)
		return 7; // v7 reserved
	else if (myguid == V8_GUID)
		return 8; // v8 reserved
	else if (myguid == V9_GUID)
		return 9; // v9 reserved
	// is it really that bad.
	else
		return -1;
}

Guid VersionToGUID(int version)
{
	switch (version)
	{
		case 1:
			return V1_GUID;
		case 2:
			return V2_GUID;
		case 3:
			return V3_GUID;
		case 4:
			return V4_GUID;
		case 5:
			return V5_GUID;
		case 6:
			return V6_GUID;
		case 7:
			return V7_GUID;
		case 8:
			return V8_GUID;
		case 9:
			return V9_GUID;
		default:
			return Guid.Empty;
	}
}

string g_AssetsPath;
string g_GameFolder;
float g_CurrentProgress;
float g_TotalProgress;

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

void DoProgress(string progressName) {
	++g_CurrentProgress;
	UpdateProgressBar(
		progressName,
		"...",
		MathF.Floor((g_CurrentProgress / g_TotalProgress) * 100.0f), 
		100.0f
	);
}

void SmartFontReplace(string origName, string scapegoatName) {
	string fontGmx = Path.Combine(g_AssetsPath, origName + ".font.gmx");
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
				fontPng = Path.Combine(g_AssetsPath, GmxString(xnode.InnerText));
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
	
	foreach (var ktuple in kpairslist) {
		foreach (var theglyph in ufont.Glyphs) {
			if (ktuple.Item1/*:mychar*/ == theglyph.Character) {
				// found our kerning pair:
				theglyph.Kerning.Add(new UndertaleFont.Glyph.GlyphKerning() {
					Other = checked((short)ktuple.Item2/*:other*/),
					Amount = checked((short)ktuple.Item3/*:amount*/)
				});
			}
		}
	}
	
	// obtain font texture
	var myFontTexture = TextureWorker.ReadImageFromFile(fontPng);
	
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
	targetFontTexture.ReplaceTexture(myFontTexture, true);
	
	// should be done?
}

void DoFonts() {
	DoProgress("Fonts");
	// null - the italian texture is equal to, or smaller.
	// otherwise, a japanese font is specified to abuse it's large AF texture.
	// (the JP texture will be wiped out, and italian font will be drawn at 0;0)
	SmartFontReplace("fnt_tinynoelle", null);
	SmartFontReplace("fnt_dotumche", "fnt_ja_dotumche"); // doesn't seem to fit... :(
	// okay now we HAVE to import this font.
	SmartFontReplace("fnt_mainbig", null);
	SmartFontReplace("fnt_main", null);
	SmartFontReplace("fnt_comicsans", null);
	SmartFontReplace("fnt_small", null);
}

void DoExportStrings() {
	Regex varNameExpr = new Regex("^[a-zA-Z_][a-zA-Z0-9_]*$");
	StringBuilder sb = new StringBuilder();
	
	foreach (var utstr in Data.Strings) {
		var asStr = utstr.Content;
		var doWrite = !varNameExpr.IsMatch(asStr);
		doWrite = doWrite
		&& !asStr.Contains("#define")
		&& !asStr.StartsWith("@@")
		&& !asStr.Contains("global.")
		&& !asStr.Contains("$$$temp")
		&& !asStr.Contains(".ogg")
		&& !asStr.Contains(".ini")
		&& !asStr.Contains(".json")
		&& !asStr.Contains(".txt")
		&& !asStr.Contains("// GameMaker")
		&& !asStr.Contains("Compatibility_Instances");
		
		if (doWrite) {
			// escape the newlines only
			asStr = asStr.Replace("\n", "\\n").Replace("\r", "\\r");
			sb.AppendLine(asStr);
		}
	}
	
	var sbs = sb.ToString();
	
	var itTXT = Path.Combine(g_AssetsPath, "stringLookup_IT.txt");
	var enTXT = Path.Combine(g_AssetsPath, "stringLookup_EN.txt");
	if (!File.Exists(itTXT) && !File.Exists(enTXT)) {
		File.WriteAllText(itTXT, sbs);
		File.WriteAllText(enTXT, sbs);
	}
	else {
		ScriptMessage("String files already exist, please delete all of them and run me again.");
	}	
}

void GetCorrectStringsPaths(out string path_base, out string path_to) {
	var path = g_AssetsPath;

	var latestV = "V" + myVersion;
	path_to = Path.Combine(path, "game_strings" + latestV + ".txt");

	var baseV = "V" + translationVersion;
	path_base = Path.Combine(path, "game_strings" + baseV + ".txt");
}

void DoImportStrings() {
	// load the files:
	var itTXT = "<!error!>";
	var enTXT = "<!error!>";
	GetCorrectStringsPaths(out enTXT, out itTXT);
	
	// sanity
	if (!File.Exists(itTXT) || !File.Exists(enTXT)) {
		ScriptError("String lookup files do not exist, they will not be imported.", "Strings Import Error");
		return;
	}
	
	string[] itLINES = File.ReadAllLines(itTXT);
	string[] enLINES = File.ReadAllLines(enTXT);
	// build a lookup table for replacement:
	Dictionary<string, string> lookup = new Dictionary<string, string>();
	
	// the fun
	int len = itLINES.Length;
	if (len != enLINES.Length) {
		//check if we have added lines
		var path = g_AssetsPath;
		var eng_path = Path.Combine(path, "game_strings" + "V0" + ".txt");
		if (!File.Exists(eng_path))
		{
			ScriptError("String lookup files do not exist, they will not be imported.", "Strings Import Error");
			return;
		}

		string[] enOriginalLINES = File.ReadAllLines(eng_path);
		if (len == enOriginalLINES.Length && len > enLINES.Length)
		{
			// we have added lines! let's update the base file with these
			for (int i = 0; i < enLINES.Length; ++i)
				enOriginalLINES[i] = enLINES[i];

			enLINES = enOriginalLINES;
		}
		else
			ScriptMessage("WARN: IT lines count does not match EN lines count!");
	}
	
	for (int l = 0; l < len; ++l) {
		lookup[enLINES[l].Replace("\\n", "\n").Replace("\\r", "\r")] = itLINES[l].Replace("\\n", "\n").Replace("\\r", "\r");
	}
	
	// do the fun:
	var abc = 0;
	foreach (var utstr in Data.Strings) {
		// if str is present in english list
		if (lookup.ContainsKey(utstr.Content)) {
			// str = italian_lookup[enlish]
			utstr.Content = lookup[utstr.Content];
			
			++abc;
		}
	}
	
	// we're done here...
	ScriptMessage("Replaced " + abc.ToString() + " strings...");
}

void DoStrings() {
	DoProgress("Strings");
	// uncomment this if you want...
	// DoExportStrings();
	DoImportStrings();
}

void DoSprites() {
	DoProgress("Sprites");
	
	var spritesFolder = Path.Combine(g_AssetsPath, "Sprites");
	if (!Directory.Exists(spritesFolder)) {
		ScriptError("Sprite folder does not exist, they will not be imported.", "Sprite Import Error");
		return;
	}
	
	var did = 0;
	
	foreach (var spr in Data.Backgrounds) {
		if (spr is null) continue;
		if (spr.Texture is null) continue;
		
		var pngpath = Path.Combine(spritesFolder, spr.Name.Content + ".png");
		if (!File.Exists(pngpath)) continue;
		
		// do the replacement:
		var sprt = TextureWorker.ReadImageFromFile(pngpath);
		
		spr.Texture.ReplaceTexture(sprt, true);
		++did;
		ScriptMessage("Found tileset = " + spr.Name.Content);
		
		try {
			sprt.Dispose();
		} catch {
			// ignore.
		}
	}
	
	foreach (var spr in Data.Sprites) {
		if (spr is null) continue;
		
		// needs to be indexed!
		for (var f = 0; f < spr.Textures.Count; ++f) {
			if (spr.Textures[f] is null) continue;
			
			var pngpath = Path.Combine(spritesFolder, spr.Name.Content + "_" + f.ToString() + ".png");
			if (!File.Exists(pngpath)) continue;
			
			// do the replacement:
			var sprt = TextureWorker.ReadImageFromFile(pngpath);
			
			spr.Textures[f].Texture.ReplaceTexture(sprt, true);
			++did;
			
			try {
				sprt.Dispose();
			} catch {
				// ignore.
			}
		}
	}
	
	ScriptMessage("Replaced " + did.ToString() + " sprites.");
}

void DoSoundReplace(string wavPath) {
	var fn = Path.GetFileNameWithoutExtension(wavPath);
	
	foreach (var snd in Data.Sounds) {
		if (snd is null) continue;
		
		if (snd.Name.Content == fn) {
			var myid = snd.AudioID;
			var fbytes = File.ReadAllBytes(wavPath);
			Data.EmbeddedAudio[myid].Data = fbytes;
			ScriptMessage("Sound found " + fn);
			return;
		}
	}
}

void DoSounds() {
	DoProgress("Sounds");
	
	var sndFolder = Path.Combine(g_AssetsPath, "Sounds");
	if (!Directory.Exists(sndFolder)) {
		ScriptError("Sound folder does not exist, they will not be imported.", "Sound Import Error");
		return;
	}
	
	var dfPath = Path.Combine(sndFolder, "dontforget_IT.ogg");
	
	var musFolderO = Path.Combine(g_GameFolder, "mus");
	var dfPathO = Path.Combine(musFolderO, "dontforget.ogg");
	
	// delete english dontforget
	File.Delete(dfPathO);
	// copy new one
	File.Copy(dfPath, dfPathO);
	
	// nik wuz here
	DoSoundReplace(Path.Combine(sndFolder, "snd_joker_anything_ch1.wav"));
	DoSoundReplace(Path.Combine(sndFolder, "snd_joker_byebye_ch1.wav"));
	DoSoundReplace(Path.Combine(sndFolder, "snd_joker_chaos_ch1.wav"));
	DoSoundReplace(Path.Combine(sndFolder, "snd_joker_metamorphosis_ch1.wav"));
	DoSoundReplace(Path.Combine(sndFolder, "snd_joker_neochaos_ch1.wav"));
}

void DoFiles() {
	DoProgress("Files");
	
	var langFolder = Path.Combine(g_GameFolder, "lang");
	
	var langch1 = Path.Combine(langFolder, "lang_en_ch1.json");
	var langch1IT = Path.Combine(g_AssetsPath, "lang_it_ch1.json");
	
	File.Delete(langch1);
	File.Copy(langch1IT, langch1);
}

UndertaleCode GetCode(string name) {
	foreach (var c in Data.Code) {
		if (c.Name.Content == name) return c;
	}
	
	throw new Exception("Unable to find code! " + name);
}

UndertaleResourceById<UndertaleString, UndertaleChunkSTRG> MakeString(string contents) {
	var obj = Data.Strings.MakeString(contents);
	var ind = Data.Strings.IndexOf(obj);
	if (ind < 0) throw new Exception("Unable to obtain resource id... " + contents);
	return new UndertaleResourceById<UndertaleString, UndertaleChunkSTRG>(obj, ind);
}

UndertaleInstruction.Reference<UndertaleVariable> MakeVarRef(string varname) {
	foreach (var vv in Data.Variables) {
		if (vv.Name.Content == varname) return new UndertaleInstruction.Reference<UndertaleVariable>(vv, UndertaleInstruction.VariableType.Normal);
	}
	
	throw new Exception("Unable to create var ref... " + varname);
}

void DoCode() {
	DoProgress("Code");
	
	// keyboard puzzle.
	
	// obj_ch2_keyboardpuzzle_tile: Variable Definitions
	GetCode("gml_Object_obj_ch2_keyboardpuzzle_tile_PreCreate_0").Instructions
		[2].Value = MakeString("M"); // push.s "M";
	
	// room_dw_cyber_keyboard_puzzle_1: Variable Definitions
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_1_1_PreCreate").Instructions
		[0].Value = MakeString("E"); // push.s "E";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_1_2_PreCreate").Instructions
		[0].Value = MakeString("L"); // push.s "L";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_1_3_PreCreate").Instructions
		[0].Value = MakeString("A"); // push.s "A";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_1_4_PreCreate").Instructions
		[0].Value = MakeString("A"); // push.s "A";
	
	// room_dw_cyber_keyboard_puzzle_2: Variable Definitions
	var instrsA = new List<UndertaleInstruction>() {
		// push.s "A";
		new UndertaleInstruction() {
			Value = MakeString("A"),
			Kind = UndertaleInstruction.Opcode.Push,
			Type1 = UndertaleInstructionUtil.FromOpcodeParam("s"),
			Type2 = UndertaleInstructionUtil.FromOpcodeParam("d"),
			TypeInst = UndertaleInstruction.InstanceType.Undefined
		},
		// pop.v.s self.myString;
		new UndertaleInstruction() {
			Destination = MakeVarRef("myString"),
			Kind = UndertaleInstruction.Opcode.Pop,
			Type1 = UndertaleInstructionUtil.FromOpcodeParam("v"),
			Type2 = UndertaleInstructionUtil.FromOpcodeParam("s"),
			TypeInst = UndertaleInstruction.InstanceType.Self
		}
	};
	
	// self.myString = "A";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_1_PreCreate")
		.Append(instrsA);
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_2_PreCreate").Instructions
		[0].Value = MakeString("N"); // push.s "N";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_3_PreCreate").Instructions
		[0].Value = MakeString("A"); // push.s "A";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_4_PreCreate").Instructions
		[0].Value = MakeString("N"); // push.s "N";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_5_PreCreate").Instructions
		[0].Value = MakeString("C"); // push.s "C";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_6_PreCreate").Instructions
		[0].Value = MakeString("S"); // push.s "S";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_7_PreCreate").Instructions
		[0].Value = MakeString("O"); // push.s "O";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_8_PreCreate").Instructions
		[0].Value = MakeString("E"); // push.s "E";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_9_PreCreate").Instructions
		[0].Value = MakeString("M"); // push.s "M";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_10_PreCreate").Instructions
		[0].Value = MakeString("T"); // push.s "T";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_11_PreCreate").Instructions
		[0].Value = MakeString("C"); // push.s "C";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_12_PreCreate").Instructions
		[0].Value = MakeString("S"); // push.s "S";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_13_PreCreate").Instructions
		[0].Value = MakeString("O"); // push.s "O";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_14_PreCreate").Instructions
		[0].Value = MakeString("T"); // push.s "T";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_15_PreCreate").Instructions
		[0].Value = MakeString("D"); // push.s "D";
		
	
	var instrsI = new List<UndertaleInstruction>() {
		// push.s "I";
		new UndertaleInstruction() {
			Value = MakeString("I"),
			Kind = UndertaleInstruction.Opcode.Push,
			Type1 = UndertaleInstructionUtil.FromOpcodeParam("s"),
			Type2 = UndertaleInstructionUtil.FromOpcodeParam("d"),
			TypeInst = UndertaleInstruction.InstanceType.Undefined
		},
		// pop.v.s self.myString;
		new UndertaleInstruction() {
			Destination = MakeVarRef("myString"),
			Kind = UndertaleInstruction.Opcode.Pop,
			Type1 = UndertaleInstructionUtil.FromOpcodeParam("v"),
			Type2 = UndertaleInstructionUtil.FromOpcodeParam("s"),
			TypeInst = UndertaleInstruction.InstanceType.Self
		}
	};
	
	// // self.myString = "I";
	GetCode("gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_16_PreCreate")
		.Append(instrsI);
	
	// end.
}

void Done() {
	DoProgress("Versioning");
	Data.GeneralInfo.DirectPlayGuid = VersionToGUID(myVersion);
	
	HideProgressBar();
	ScriptMessage("Done. PLEASE save and overwrite your file (Ctrl+S) and run the game!");
}

void ScriptEntry() {
	// TODO: translate script to le pasta?
	// only work with actual data files please.
	EnsureDataLoaded();
	
	g_CurrentProgress = 0.0f;
	g_TotalProgress = 7.0f; // UPDATE THIS
	
	if (ScriptPath is null) {
		ScriptError("This script can only be ran as a file on disk.", "Assets Error");
		return;
	}
	
	g_AssetsPath = Path.Combine(Path.GetDirectoryName(ScriptPath), "Assets");
	if (!Directory.Exists(g_AssetsPath)) {
		ScriptError("Translation assets directory does not exist. Please unpack the whole zip archive!", "Assets Error");
		return;
	}
	
	g_GameFolder = Path.GetDirectoryName(FilePath);
	// check for `mus` folder's presence (needed for sound replacement)
	if (!Directory.Exists(Path.Combine(g_GameFolder, "mus"))) {
		ScriptError("Game's folder does not seem to be a valid one.", "Assets Error");
		return;
	}
	
	var gameName = Data.GeneralInfo?.Name?.Content;
	if (gameName != "DELTARUNE") {
		ScriptError("This game is not the 'Chapter 1&2' demo.", "Game Error");
		return;
	}
	
	translationVersion = DetermineTranslationVersion(Data);
	if (translationVersion < 0) {
		ScriptError("Unable to detect .win translation version.", "Version Error");
		return;
	}
	
	if (translationVersion > myVersion) {
		ScriptError("This .win contains a translation from the future, update the installer.", "Version Error");
		return;
	}
	
	if (translationVersion == myVersion) {
		ScriptError("Translation is already applied, no need to.", "Version Error");
		return;
	}
	
	if (translationVersion > 0) {
		ScriptMessage("Updating the translation...");
	}
	
	var doPatch = true;
	// the API says nothing against that :p
	if (DummyString() != "<usp_installer>") doPatch = ScriptQuestion("This will modify your .win file, proceed?");
	if (!doPatch) return;
	
	DoStrings();
	DoSprites();
	DoFonts();
	DoSounds();
	DoFiles();
	DoCode();
	
	Done();
}	

/** the script starts here */
ScriptEntry();

