/**
 *  Undertale Spaghetti Project
 *  DELTARUNE Chapter 4 Translation script
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

readonly Version g_supportedVersion = new Version(0, 0, 110);

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
	ImportLangJson(Path.Join(assetPath, "Strings", "chapter4", "lang_it_ch4.json"), gameFolder);
	ImportHardcodedStrings(
		Path.Join(assetPath, "Strings", "chapter4", "lookup_en_ch4.txt"),
		Path.Join(assetPath, "Strings", "chapter4", "lookup_it_ch4.txt")
	);
	
	await Progress("Code");
	ImportAllCode(Path.Join(assetPath, "Codes"));
	UpdateItemGetCode();
	UpdateWaterCoolerCode("gml_Object_obj_holywatercooler_enemy_Step_0");
	UpdateTemmieSongCode();
	UpdateRankStringCode();

	await Progress("Sprites");
	ImportAllSprites(Path.Join(assetPath, "Sprites"));
	ImportAllTilesets(Path.Join(assetPath, "Sprites", "Backgrounds"));

	await Progress("Fonts");
	ImportAllFontGlyphs(Path.Join(assetPath, "Fonts"));
	ImportAllFontGraphics(Path.Join(assetPath, "Fonts"));

	await Progress("Sounds");
	ImportSounds(Path.Join(assetPath, "Sounds"));

	Done();
}

await Main();