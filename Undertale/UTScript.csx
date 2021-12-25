/** usings */
﻿using System;
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
string trfolderName = "Assets";
string[] stringArray = new string[(int)StrType._LENGTH];
string RealScriptPath = Path.GetDirectoryName(ScriptPath);
double progValue = 0.0;
double totalValue = 5.0;

/**
 * -1 - error/undefined
 *  0 - no translation (english)
 *  1 - translation V1 (no Guid set, but Italian strings applied)
 *  2 - translation V2 (Guid set)
 *  3 - translation V3 (corrected Italian)
 *  4 and onward - reserved.
 */
int translationVersion = -1;
int myVersion = 3; // this installer is V3.

static Guid V1_GUID = Guid.Empty;
static Guid V2_GUID = Guid.Parse("{7DF3449D-34E6-4D1D-A2E6-175F989891F2}");
static Guid V3_GUID = Guid.Parse("{EB2E8648-35B5-4344-B2E0-1B6894BDEA32}");
static Guid V4_GUID = Guid.Parse("{5F9253BD-D031-4BEB-AEAB-AB3C1740533F}"); // reserved
static Guid V5_GUID = Guid.Parse("{83D3ECDE-3B53-40AE-AA30-3D04FAF72832}"); // reserved
static Guid V6_GUID = Guid.Parse("{E18F6929-E1ED-4A07-8486-DAA2A6725B81}"); // reserved

int DetermineTranslationVersion(UndertaleData dd)
{
	Guid myguid = dd.GeneralInfo.DirectPlayGuid;
	if (myguid == V1_GUID)
	{
		// I left this message intentionally in `attention_hackerz_no_2` variable `demond`
		// it turns out my weird joke *is* useful now.
		if (dd.Strings.Any(str => str.Content == "pleaz no mayo pasta (eat mayo pasta!!!)"))
			return 1; // v1
		else
			return 0; // english
	}
	else if (myguid == V2_GUID)
		return 2; // v2
	else if (myguid == V3_GUID)
		return 3; // v3 reserved
	else if (myguid == V4_GUID)
		return 4; // v4 reserved
	else if (myguid == V5_GUID)
		return 5; // v5 reserved
	else if (myguid == V6_GUID)
		return 6; // v6 reserved
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
		default:
			return Guid.Empty;
	}

}

string VersionToReleaseVersion(int version)
{
	switch (version)
	{
		case 1:
			return "V1.11";
		case 2:
			return "V1.15";
		case 3:
			return "V1.16";
		case 4:
		case 5:
		case 6:
			return "VX.XX";
		default:
			return "Invalid";
	}
}

void ExecuteTranslation(string paath)
{
	EnsureDataLoaded(); // so we only work with actual files.
	stringArray[(int)StrType.NOT_UT_ERROR] = "Il gioco che vuoi tradurre non é UNDERTALE...\r\n Sei sicuro di stare bene :(?";
	stringArray[(int)StrType.NO_ASSET_ERROR] = "Mancano alcune risorse necessarie per la traduzione traduzione!\r\n Perfavore riscarica USP.";
	stringArray[(int)StrType.WANT_TRANSLATE] = "Adesso il gioco verrá'tradotto\r\n";
	stringArray[(int)StrType.FMT_AN_ERROR] = "Durante l'esecuzione é stato lanciato il seguente errore {0}:\r\n{1}";
	stringArray[(int)StrType.DONE] = "Concludo il processo...";
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

	string path = Path.Combine(RealScriptPath, trfolderName);
	string winpath = Path.GetDirectoryName(FilePath);
	
	bool isUndertale = (Data.GeneralInfo.DisplayName.Content.ToLower() == "undertale" || Data.GeneralInfo.DisplayName.Content.ToLower() == "nxtale");
	// i don't think nxtale will work but idfk lol.
	// UPD: it works... sorta...

	if (!isUndertale)
	{
		ScriptError(stringArray[(int)StrType.NOT_UT_ERROR], stringArray[(int)StrType.ERROR_TITLE]);
		return;
	}

	if (!Directory.Exists(path))
	{
		ScriptError(stringArray[(int)StrType.NO_ASSET_ERROR], stringArray[(int)StrType.ERROR_TITLE]);
		return;
	}


	translationVersion = DetermineTranslationVersion(Data);
	if (translationVersion < 0)
	{
		ScriptMessage(string.Format(stringArray[(int)StrType.TOONEW_ERR]));
		return;
	}

	if (translationVersion > myVersion)
	{
		ScriptMessage(string.Format(stringArray[(int)StrType.VERSION_ERR], VersionToReleaseVersion(translationVersion), VersionToReleaseVersion(myVersion)));
		return;
	}

	if (translationVersion == myVersion)
	{
		ScriptMessage(string.Format(stringArray[(int)StrType.NO_NEED_TO], VersionToReleaseVersion(translationVersion)));
		return;
	}

	if (translationVersion > 0)
	{
		ScriptMessage(stringArray[(int)StrType.YOURE_UPDATING]);
	}

	ImportSounds();
	ImportSprites();
	ImportFonts();
	ImportStrings();
	ImportSplash();
	Finish();
}

void UpdateProgress(string n)
{
	++progValue;

	ScriptMessage("Progress: " + (progValue / totalValue) * 100  + " / " + (totalValue / totalValue) * 100 + " " + n );
}


// --- Import Sounds --- //
void ImportSounds()
{
	string path = Path.Combine(RealScriptPath, trfolderName);
	string winpath = Path.GetDirectoryName(FilePath);

	UpdateProgress("Sounds...");
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
		wonderfulidea = null; // free the array since we don't need it anymore.

		// Copy Mettaton sound.
		File.Delete(Path.Combine(winpath, "mus_ohyes.ogg"));
		File.Copy(Path.Combine(path, "mus_ohyes.ogg"), Path.Combine(winpath, "mus_ohyes.ogg"));

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

// --- Import Sprites --- //
void ImportSprites()
{
	string path = Path.Combine(RealScriptPath, trfolderName);
	string winpath = Path.GetDirectoryName(FilePath);
	UpdateProgress("Sprites...");
	
	try
	{
		// no need to apply sprites.
		if (translationVersion >= 1)
		{
			ScriptMessage(string.Format(stringArray[(int)StrType.NO_NEED_TO], stringArray[(int)StrType.SPRITES]));
			return;
		}

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

				var tex = TextureWorker.ReadImageFromFile(fullname);
				spr.Textures[t].Texture.ReplaceTexture(tex);
			}
		}
	}
	catch (Exception e)
	{
		throw new Exception(string.Format(stringArray[(int)StrType.FMT_AN_ERROR], stringArray[(int)StrType.SPRITES], e.ToString()));
	}
}

// --- Import Fonts --- //
void ImportFonts()
{
	string path = Path.Combine(RealScriptPath, trfolderName);
	string winpath = Path.GetDirectoryName(FilePath);
	UpdateProgress("Fonts... (ci impiegherá un po')");
	try
	{
		// no need to apply fonts.
		if (translationVersion >= 1)
		{
			ScriptError(stringArray[(int)StrType.NO_NEED_TO], stringArray[(int)StrType.FONTS]);
			return;
		}

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

		tpage1.TexturePage.TextureData.TextureBlob = File.ReadAllBytes(fonttexpath1);
		//tpage2.TexturePage.TextureData.TextureBlob = File.ReadAllBytes(fonttexpath2);
		// ^ behaves weird on PSVita when replacing an entire page...
		// ^^ button sprites get messed up... (perhaps different positions??????)
		//8-4 Ltd what are you doing

		// there we go, an alternative method to inject gaster font.
		var wingding = TextureWorker.ReadImageFromFile(fonttexpath2);
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
	catch (Exception e)
	{
		throw new Exception(string.Format(stringArray[(int)StrType.FMT_AN_ERROR], stringArray[(int)StrType.FONTS], e.ToString()));
	}
}

void ImportStrings()
{
	string path = Path.Combine(RealScriptPath, trfolderName);
	string winpath = Path.GetDirectoryName(FilePath);
	UpdateProgress("Strings...");
	try
	{
		GetCorrectStringsPaths(out string base_path, out string to_path);
		string[] cont1 = File.ReadAllLines(base_path);
		string[] cont2 = File.ReadAllLines(to_path);

		var locDict = StringArraysToDic(cont1, cont2);

		for (int s = 0; s < Data.Strings.Count; s++)
		{
			var str = Data.Strings[s].Content;

			if (locDict.ContainsKey(str))
				Data.Strings[s].Content = locDict[str];
		}

		//locDict.Dispose();
		cont1 = null;
		cont2 = null;
	}
	catch (Exception e)
	{
		throw new Exception(string.Format(stringArray[(int)StrType.FMT_AN_ERROR], stringArray[(int)StrType.STRINGS], e.ToString()));
	}
}

void ImportSplash()
{

	string path = Path.Combine(RealScriptPath, trfolderName);
	string winpath = Path.GetDirectoryName(FilePath);
	UpdateProgress("Splash...");

	try
	{
		string utsplashPath = Path.Combine(winpath, "splash.png");
		// the file is already 640x480 (right resolution for game window!)
		string ITsplashPath = Path.Combine(path, "OPTIONAL_splash.png");
		if (File.Exists(utsplashPath) && File.Exists(ITsplashPath))
		{
			File.Delete(utsplashPath);
			File.Copy(ITsplashPath, utsplashPath);
		}
		// that should be it...
	}
	catch (Exception e)
	{
		// silently ignore any errors since it's not important.
	}
}

// --- A little helper --- //

Dictionary<string, string> StringArraysToDic(string[] arr1, string[] arr2)
{
	Dictionary<string, string> dic = new Dictionary<string, string>();
	for (int s = 0; s < arr2.Length; s++)
	{
		dic.Add(arr1[s].Replace(@"\n", "\n").Replace(@"\r", "\r"), arr2[s].Replace(@"\n", "\n").Replace(@"\r", "\r"));
		//        ^^^ allows to properly unescape newlines and stuff.
	}
	return dic;
}

void GetCorrectStringsPaths(out string path_base, out string path_to)
{
	string path = Path.Combine(RealScriptPath, trfolderName);

	string latestV = "V" + myVersion;
	path_to = Path.Combine(path, "game_strings" + latestV + ".txt");

	string baseV = "V" + translationVersion;
	path_base = Path.Combine(path, "game_strings" + baseV + ".txt");
}

// --- Done, hooray! --- //
void Finish()
{
	// write new GUID
	Data.GeneralInfo.DirectPlayGuid = VersionToGUID(myVersion);

	ScriptMessage(stringArray[(int)StrType.DONE]);
	// do something here...?
}

ExecuteTranslation(RealScriptPath);
