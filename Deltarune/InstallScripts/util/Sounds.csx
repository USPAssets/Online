using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UndertaleModLib.Util;
using ImageMagick;

void ImportSounds(string sndFolder, string suffix = null)
{
	if (!Directory.Exists(sndFolder)) {
		ScriptError("Sound folder does not exist, they will not be imported.", "Sound Import Error");
		return;
	}
	
	foreach (var snd in Data.Sounds) {
		// First try .wav extension
		var sndPath = Path.Join(sndFolder, snd.Name.Content + ".wav");
		if (!File.Exists(sndPath) && suffix != null && snd.Name.Content.EndsWith(suffix)) {
			// Try with suffix added if it doesn't exist
			string strippedName = snd.Name.Content.Substring(0, snd.Name.Content.Length - suffix.Length);
			sndPath = Path.Join(sndFolder, strippedName + ".wav");
		}

		// Also try .ogg files, which may also be embedded
		if (!File.Exists(sndPath)) {
			sndPath = Path.Join(sndFolder, snd.Name.Content + ".ogg");
		}

		if (File.Exists(sndPath)) {
			var myid = snd.AudioID;
			var fbytes = File.ReadAllBytes(sndPath);
			Data.EmbeddedAudio[myid].Data = fbytes;
		}
	}
}

// Imports .ogg files into "mus" folder.
// To be called only by launcher script.
void ImportMusic(string sndFolder, string gameFolder)
{
	if (!Directory.Exists(sndFolder)) {
		ScriptError("Sound folder does not exist, they will not be imported.", "Sound Import Error");
		return;
	}

	foreach (string sndFile in Directory.GetFiles(sndFolder))
    {
        if (!sndFile.EndsWith(".ogg")) continue;
		var destPath = Path.Join(gameFolder, "mus", Path.GetFileName(sndFile));
		if (File.Exists(destPath))
		{
			File.Copy(sndFile, destPath, true);
		}
	}
}