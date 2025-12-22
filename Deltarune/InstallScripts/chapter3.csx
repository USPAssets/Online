/**
 *  Undertale Spaghetti Project
 *  DELTARUNE Chapter 3 Translation script
 *
 *  @author USP
 */

#load "util/Fonts.csx"
#load "util/GameCode.csx"
#load "util/Graphics.csx"
#load "util/Sounds.csx"
#load "util/Strings.csx"
#load "util/Version.csx"

using UndertaleModLib.Util; // TextureWorker
using System.Text; // StringBuilder
using System.Xml; // XmlDocument
using System.Threading.Tasks;
using System.Text.RegularExpressions; // Regex
using System.Linq;
using System.IO;
using ImageMagick;

void Done() {
	HideProgressBar();
	ScriptMessage("Done. PLEASE save and overwrite your file (Ctrl+S) and run the game!");
}

void ImportVideo(string videoFolder, string gameFolder, string fileName) {
	var sourcePath = Path.Join(videoFolder, fileName);
	var destPath = Path.Join(gameFolder, "vid", fileName);
	if (!File.Exists(sourcePath) || !File.Exists(destPath)) {
		ScriptError("Video file does not exist, it will not be imported.", "Videos Import Error");
		return;
	}
	// copy new one over old one
	File.Copy(sourcePath, destPath, true);
}

readonly Version g_supportedVersion = new Version(0, 0, 98);

async Task Main()
{
	EnsureDataLoaded();

	if (ScriptPath is null) {
		ScriptError("This script can only be ran as a file on disk.", "Assets Error");
		return;
	}

	var gameName = Data.GeneralInfo?.Name?.Content;
	if (gameName != "DELTARUNE") {
		throw new Exception("Questo gioco non è DELTARUNE, ma è: " + gameName);
	}

	string assetPath = Directory.GetParent(Path.GetDirectoryName(ScriptPath)).FullName;
	if (!Directory.Exists(assetPath)) {
		throw new Exception("Non trovo la cartella degli asset di traduzione. Assicurati di aver estratto l'archivio ZIP completo!");
	}

	string gameFolder = Path.GetDirectoryName(FilePath);
	// check for `lang` folder's presence
	if (!Directory.Exists(Path.Combine(gameFolder, "lang"))) {
		throw new Exception("La cartella di gioco non ha il formato corretto. Sei sicuro di aver scelto il percorso giusto?");
	}

	string? gameVersion = FindCodeVariableValue("gml_Object_obj_initializer2_Create_0", "global.versionno");
	CheckGameVersion(g_supportedVersion, gameVersion);

	int numSteps = 6;
	int currentStep = 1;
	async Task Progress(string progressName) {
		UpdateProgressBar(
			progressName,
			"...",
			currentStep++, numSteps
		);
		await Task.Yield(); // Allow UI to update
	}

	await Progress("Strings");
	ImportLangJson(Path.Join(assetPath, "Strings", "chapter3", "lang_it_ch3.json"), gameFolder);
	ImportHardcodedStrings(
		Path.Join(assetPath, "Strings", "chapter3", "lookup_en_ch3.txt"),
		Path.Join(assetPath, "Strings", "chapter3", "lookup_it_ch3.txt")
	);
	// Hardcoded strings which are also variable names
	ReplaceStringInCode("gml_Object_obj_tenna_enemy_bg_Draw_0", "bet", "posta");
	
	await Progress("Code");
	ImportAllCode(Path.Join(assetPath, "Codes"));
	UpdateItemGetCode();
	UpdateWaterCoolerCode();
	UpdateRankStringCode();

	await Progress("Sprites");
	ImportAllGraphics(Path.Join(assetPath, "Sprites"));
	// Update spr_funnytext_coffee origin to +152 +32 (same as spr_ja_funnytext_coffee)
	var funnyTextSprite = Data.Sprites.ByName("spr_funnytext_coffee");
	funnyTextSprite.OriginX = 152;
	funnyTextSprite.OriginY = 32;
	// Update spr_funnytext_know_tv origin to +176 +34 (same as spr_ja_funnytext_know_tv)
	funnyTextSprite = Data.Sprites.ByName("spr_funnytext_know_tv");
	funnyTextSprite.OriginX = 176;
	funnyTextSprite.OriginY = 34;
	// Update spr_funnytext_tan origin to +151 +32 (same as spr_ja_funnytext_tan)
	funnyTextSprite = Data.Sprites.ByName("spr_funnytext_tan");
	funnyTextSprite.OriginX = 151;
	funnyTextSprite.OriginY = 32;
	// Update spr_funnytext_alligator origin to +206 +49 (custom origin to fit bigger sprite)
	funnyTextSprite = Data.Sprites.ByName("spr_funnytext_alligator");
	funnyTextSprite.OriginX = 206;
	funnyTextSprite.OriginY = 49;
	// Update spr_funnytext_rock_concert origin to +112 +30 (same as spr_ja_funnytext_rock_concert)
	funnyTextSprite = Data.Sprites.ByName("spr_funnytext_rock_concert");
	funnyTextSprite.OriginX = 112;
	funnyTextSprite.OriginY = 30;
	// Update spr_funnytext_fun_loop origin to +151 +26 (same as spr_ja_funnytext_fun_loop_0)
	funnyTextSprite = Data.Sprites.ByName("spr_funnytext_fun_loop");
	funnyTextSprite.OriginX = 151;
	funnyTextSprite.OriginY = 26;

	await Progress("Video");
	ImportVideo(Path.Join(assetPath, "Videos"), gameFolder, "tennaIntroF1_compressed_28.mp4");

	await Progress("Fonts");
	var fontsPath = Path.Join(assetPath, "Fonts");
	ImportAllFonts(fontsPath);
	SmartFontReplace(fontsPath, "fnt_8bit", null);

	await Progress("Sounds");
	ImportSounds(Path.Join(assetPath, "Sounds"));

	Done();
}

await Main();