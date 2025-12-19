using UndertaleModLib.Util; // TextureWorker
using System.Text; // StringBuilder
using System.Xml; // XmlDocument
using System.Threading.Tasks;
using System.Text.RegularExpressions; // Regex
using System.Linq;
using System.IO;
using ImageMagick;

void CheckGameVersion(Version expectedVersion, string? value, Version? fallbackVersion = null)
{
    if (value == null)
    {
        if (!ScriptQuestion(
@"Non siamo stati in grado di determinare la versione del gioco.
Molto probabilmente, il gioco è stato aggiornato e la traduzione non è più compatibile. 
Vuoi continuare lo stesso? (Potrebbe non funzionare correttamente)"))
        {
            throw new Exception("Errore: versione del gioco non trovata. Installazione interrotta.");
        }
        return;
    }

    // Strip possible v prefix (used for ch3 & ch4)
    if (value.StartsWith("v")) {
        value = value.Substring(1);
    }
    Version foundVersion = new Version(value);
    if (foundVersion < expectedVersion)
    {
        if (foundVersion == fallbackVersion)
        {
            return;
        }
        throw new Exception($"Stai usando una versione vecchia del gioco ({foundVersion}). Aggiornala subito all'ultima versione ({expectedVersion}).");
    }
    if (foundVersion > expectedVersion)
    {
        if (!ScriptQuestion(
@"La versione del gioco è più recente rispetto a quella che la traduzione supporta attualmente.
Vuoi continuare lo stesso? (Potrebbe non funzionare correttamente)"))
        {
            throw new Exception("Errore: versione del gioco non supportata. Installazione interrotta.");
        }
    }
}