using UndertaleModLib.Util; // TextureWorker
using System.Text; // StringBuilder
using System.Xml; // XmlDocument
using System.Threading.Tasks;
using System.Text.RegularExpressions; // Regex
using System.Linq;
using System.IO;
using ImageMagick;

void ImportLangJson(string langFilePath, string gameFolder, string destName = "lang_it.json") {
    if (!Directory.Exists(Path.Join(gameFolder, "lang"))) {
        ScriptError("Game's folder does not seem to be a valid one.", "Assets Error");
        return;
    }	
	var destination = Path.Join(gameFolder, "lang", destName);
	File.Copy(langFilePath, destination, true);
}

void ImportHardcodedStrings(string lookupOriginalPath, string lookupTranslatedPath)
{
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