/**
 *  Undertale Spaghetti Project
 *  DELTARUNE Chapter 2 Translation script
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

readonly Version g_supportedVersion = new Version(1, 10);

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

	string? gameVersion = FindCodeVariableValue("gml_Object_obj_CHAPTER_SELECT_Create_0", "global.version");
	CheckGameVersion(g_supportedVersion, gameVersion);

	int numSteps = 5;
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
	ImportLangJson(Path.Join(assetPath, "Strings", "chapter1", "lang_it_ch1.json"), gameFolder, "lang_it_ch1.json");
	ImportLangJson(Path.Join(assetPath, "Strings", "chapter2", "lang_it_ch2.json"), gameFolder, "lang_it.json");
	ImportHardcodedStrings(
		Path.Join(assetPath, "Strings", "chapter1", "lookup_en_ch1.txt"),
		Path.Join(assetPath, "Strings", "chapter1", "lookup_it_ch1.txt")
	);
	ImportHardcodedStrings(
		Path.Join(assetPath, "Strings", "chapter2", "lookup_en_ch2.txt"),
		Path.Join(assetPath, "Strings", "chapter2", "lookup_it_ch2.txt")
	);
	ImportHardcodedStrings(
		Path.Join(assetPath, "Strings", "launcher", "lookup_en.txt"),
		Path.Join(assetPath, "Strings", "launcher", "lookup_it.txt")
	);
	
	await Progress("Code");
	ImportAllCode(Path.Join(assetPath, "Codes"));
	UpdateKeyboardPuzzleCode();

	await Progress("Sprites");
	ImportAllGraphics(Path.Join(assetPath, "Sprites"), "_ch1");

	await Progress("Fonts");
	ImportAllFonts(Path.Join(assetPath, "Fonts"));

	await Progress("Sounds");
	var sndPath = Path.Join(assetPath, "Sounds");
	ImportSounds(sndPath, "_ch1");
	ImportMusic(sndPath, gameFolder);

	Done();
}

await Main();
