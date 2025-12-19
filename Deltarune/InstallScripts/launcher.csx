/**
 *  Undertale Spaghetti Project
 *  DELTARUNE Chapter 2 Translation script
 *
 *  @author USP
 */

#load "util/Strings.csx"
#load "util/Version.csx"
#load "util/GameCode.csx"
#load "util/Fonts.csx"
#load "util/Sounds.csx"

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

readonly Version g_supportedVersion = new Version(17, 0);
readonly Version g_itchioVersion = new Version(3, 0);

async Task Main()
{
	EnsureDataLoaded();

	if (ScriptPath is null) {
		ScriptError("This script can only be ran as a file on disk.", "Assets Error");
		return;
	}

	var gameName = Data.GeneralInfo?.Name?.Content;
	if (gameName != "DELTARUNE") {
		throw new Exception("Questo gioco non è DELTARUNE: " + gameName);
	}

	string assetPath = Directory.GetParent(Path.GetDirectoryName(ScriptPath)).FullName;
	if (!Directory.Exists(assetPath)) {
		throw new Exception("Non trovo la cartella degli asset di traduzione. Assicurati di aver estratto l'archivio ZIP completo!");
	}

	string gameFolder = Path.GetDirectoryName(FilePath);
	// check for `mus` folder's presence
	if (!Directory.Exists(Path.Combine(gameFolder, "mus"))) {
		throw new Exception("La cartella di gioco non ha il formato corretto. Sei sicuro di aver scelto il percorso giusto?");
	}

	string? gameVersion = FindCodeVariableValue("gml_GlobalScript_scr_init", "var version");
	gameVersion = gameVersion?.TrimStart('v');
	if (gameVersion != null) {
		// Ensure it is in the format "X.Y" for Version constructor
		gameVersion += ".0";
	}
	CheckGameVersion(g_supportedVersion, gameVersion, g_itchioVersion);

	int numSteps = 4;
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
	ImportHardcodedStrings(
		Path.Join(assetPath, "Strings", "launcher", "lookup_en.txt"),
		Path.Join(assetPath, "Strings", "launcher", "lookup_it.txt")
	);

	await Progress("Fonts");
	ImportAllFonts(Path.Join(assetPath, "Fonts"), false);

	await Progress("Sounds");
	var sndPath = Path.Join(assetPath, "Sounds");
	ImportSounds(sndPath);
	ImportMusic(sndPath, gameFolder);

	await Progress("Code");
	UpdateLauncherStarsPosition();

	Done();
}

await Main();