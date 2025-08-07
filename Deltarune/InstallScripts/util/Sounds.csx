using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UndertaleModLib.Util;
using ImageMagick;

void ImportSounds(string sndFolder)
{
	if (!Directory.Exists(sndFolder)) {
		ScriptError("Sound folder does not exist, they will not be imported.", "Sound Import Error");
		return;
	}
	
	foreach (var snd in Data.Sounds) {
		var wavPath = Path.Join(sndFolder, snd.Name.Content + ".wav");
		if (!File.Exists(wavPath)) {
			continue;
		}

		var myid = snd.AudioID;
		var fbytes = File.ReadAllBytes(wavPath);
		Data.EmbeddedAudio[myid].Data = fbytes;
	}
}

void ImportMusic(string assetsPath, string gameFolder)
{
	var sndFolder = Path.Join(assetsPath, "Sounds");
	if (!Directory.Exists(sndFolder)) {
		ScriptError("Sound folder does not exist, they will not be imported.", "Sound Import Error");
		return;
	}
	// To be done in launcher script
	var dfPath = Path.Join(sndFolder, "dontforget_IT.ogg");
	
	var musFolderO = Path.Join(gameFolder, "mus");
	var dfPathO = Path.Join(musFolderO, "dontforget.ogg");
	
	// copy new one over old one
	File.Copy(dfPath, dfPathO, true);
}