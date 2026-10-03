using System.Collections.Generic;
using System.Globalization;

namespace NvdaWowVoice
{
    /// <summary>English / German / French, picked from the Windows UI language
    /// (same three as SkuInstaller). Missing keys fall back to English.</summary>
    internal static class Strings
    {
        // Not initialised inline: static initialisers run in textual order, and En
        // is declared below, so it would still be null here.
        private static Dictionary<string, string> _table;

        public static void Init()
        {
            try
            {
                switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                {
                    case "de": _table = De; break;
                    case "fr": _table = Fr; break;
                }
            }
            catch { }
        }

        public static string Get(string key) =>
            (_table ?? En).TryGetValue(key, out var s) ? s : En.TryGetValue(key, out s) ? s : key;

        public static string Format(string key, params object[] args) => string.Format(Get(key), args);

        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            ["title"] = "NVDA Voice for WoW",
            ["welcome.heading"] = "Make NVDA the voice of World of Warcraft",
            ["welcome.text"] =
                "This setup lets World of Warcraft speak through NVDA. You do not need the Sku addon for it.\n\n" +
                "What it does:\n" +
                "1. Installs the NVDA bridge voice SAPI2SR into the SAPI2SR folders under Program Files.\n" +
                "2. Registers it as a Windows speech voice named SAPI2SR.\n" +
                "3. Signs the voice on this computer. World of Warcraft refuses to load unsigned voices, so without this step the voice stays silent in the game. " +
                "The certificate is created on this computer only, and its private key is deleted right after signing, so it can never sign anything else.\n" +
                "4. Makes the other Windows voices available to the game. Windows keeps its newer voices (for example Katja and Stefan for German) in a separate list the game cannot see; this copies their entries into the game's list. " +
                "The voices themselves are not changed, and the list is backed up first.\n\n" +
                "Before you start:\n" +
                "- Close World of Warcraft completely.\n" +
                "- NVDA must be running while you play.\n\n" +
                "Running this setup again repairs or updates the voice, and adds Windows voices installed since (in the Windows settings under Time and Language, Speech).\n\n" +
                "Press Install to start, or Cancel to quit without changing anything.",
            ["welcome.textAcc"] = "Description",
            ["welcome.install"] = "&Install",
            ["welcome.cancel"] = "Cancel",
            ["wowRunning"] = "World of Warcraft is still running ({0}).\n\nClose the game completely, then choose Retry. Choose Cancel to quit.",
            ["progress.heading"] = "Installing the NVDA voice. Please wait…",
            ["progress.logAcc"] = "Progress",
            ["step.extract"] = "Unpacking the voice files…",
            ["step.copy"] = "Copying the voice files to Program Files…",
            ["step.copied"] = "{0} files copied.",
            ["step.register"] = "Registering the SAPI2SR voice with Windows…",
            ["step.sign"] = "Signing the voice so World of Warcraft will load it. This can take up to a minute…",
            ["step.check"] = "Checking the installation…",
            ["step.voices"] = "Making the other Windows voices available to the game…",
            ["step.voicesNone"] = "All Windows voices were already available to the game.",
            ["step.voiceAdded"] = "Added the voice {0}.",
            ["step.voicesAdded"] = "Windows voices added: {0}.",
            ["done.ok.heading"] = "Finished. The NVDA voice is ready.",
            ["done.ok.text"] =
                "The SAPI2SR voice is installed, registered and signed, and the game can use every Windows voice of this computer.\n\n" +
                "Next steps:\n" +
                "1. Start NVDA if it is not running.\n" +
                "2. Start World of Warcraft.\n" +
                "3. In the game's text to speech settings, choose the voice SAPI2SR. " +
                "Everything the game speaks with that voice is now read by NVDA.\n\n" +
                "The game numbers its voices by their place in the Windows list, so added voices can change those numbers. " +
                "If a voice in the game or in an addon sounds different than before, choose it again.",
            ["done.fail.heading"] = "Not finished. Some steps did not work.",
            ["done.fail.text"] = "These problems occurred:\n{0}\n\nFix the cause and run this setup again.\n\nLog file: {1}",
            ["done.close"] = "&Close",
            ["done.openLog"] = "&Open log in Notepad",
            ["done.textAcc"] = "Result",
            ["err.fileLocked"] = "- A file is in use and could not be replaced: {0}. Close World of Warcraft and other programs that use Windows voices.",
            ["err.dllMissing"] = "- Voice engine file missing: {0}",
            ["err.register"] = "- Registering {0} failed ({1}).",
            ["err.sign"] = "- Signing did not finish: {0}",
            ["err.sign.noScript"] = "the signing script is missing from the package.",
            ["err.sign.2"] = "the voice files are not the expected version.",
            ["err.sign.3"] = "creating the signature failed.",
            ["err.sign.4"] = "the signature could not be verified.",
            ["err.sign.5"] = "administrator rights are missing.",
            ["err.sign.6"] = "a voice engine file is missing.",
            ["err.sign.7"] = "a voice file is in use. Close World of Warcraft completely and run this setup again.",
            ["err.notRegistered"] = "- The SAPI2SR voice is not registered with Windows.",
            ["err.voicesBackup"] = "- The voice list could not be backed up, so no Windows voices were added.",
            ["err.voice"] = "- The voice {0} could not be added ({1}).",
            ["err.voices"] = "- Adding the Windows voices failed: {0}",
            ["err.unexpected"] = "- Unexpected error: {0}",
        };

        private static readonly Dictionary<string, string> De = new Dictionary<string, string>
        {
            ["title"] = "NVDA-Stimme für WoW",
            ["welcome.heading"] = "NVDA zur Stimme von World of Warcraft machen",
            ["welcome.text"] =
                "Dieses Setup lässt World of Warcraft über NVDA sprechen. Das Sku-Addon brauchst du dafür nicht.\n\n" +
                "Was es tut:\n" +
                "1. Installiert die NVDA-Brückenstimme SAPI2SR in die SAPI2SR-Ordner unter Programme.\n" +
                "2. Registriert sie als Windows-Sprachstimme mit dem Namen SAPI2SR.\n" +
                "3. Signiert die Stimme auf diesem Computer. World of Warcraft lädt keine unsignierten Stimmen, ohne diesen Schritt bleibt die Stimme im Spiel stumm. " +
                "Das Zertifikat wird nur auf diesem Computer erzeugt, und sein privater Schlüssel wird direkt nach dem Signieren gelöscht, es kann also nie etwas anderes signieren.\n" +
                "4. Macht die übrigen Windows-Stimmen für das Spiel verfügbar. Windows führt seine neueren Stimmen (für Deutsch zum Beispiel Katja und Stefan) in einer eigenen Liste, die das Spiel nicht sieht; das Setup kopiert ihre Einträge in die Liste des Spiels. " +
                "Die Stimmen selbst bleiben unverändert, und die Liste wird vorher gesichert.\n\n" +
                "Bevor du startest:\n" +
                "- Schließe World of Warcraft vollständig.\n" +
                "- NVDA muss beim Spielen laufen.\n\n" +
                "Wenn du dieses Setup erneut ausführst, wird die Stimme repariert oder aktualisiert, und seitdem installierte Windows-Stimmen kommen dazu (in den Windows-Einstellungen unter Zeit und Sprache).\n\n" +
                "Drücke Installieren zum Starten, oder Abbrechen, um ohne Änderungen zu beenden.",
            ["welcome.textAcc"] = "Beschreibung",
            ["welcome.install"] = "&Installieren",
            ["welcome.cancel"] = "Abbrechen",
            ["wowRunning"] = "World of Warcraft läuft noch ({0}).\n\nSchließe das Spiel vollständig und wähle dann Wiederholen. Mit Abbrechen beendest du das Setup.",
            ["progress.heading"] = "Die NVDA-Stimme wird installiert. Bitte warten…",
            ["progress.logAcc"] = "Fortschritt",
            ["step.extract"] = "Entpacke die Stimmdateien…",
            ["step.copy"] = "Kopiere die Stimmdateien nach Programme…",
            ["step.copied"] = "{0} Dateien kopiert.",
            ["step.register"] = "Registriere die Stimme SAPI2SR bei Windows…",
            ["step.sign"] = "Signiere die Stimme, damit World of Warcraft sie lädt. Das kann bis zu einer Minute dauern…",
            ["step.check"] = "Prüfe die Installation…",
            ["step.voices"] = "Mache die übrigen Windows-Stimmen für das Spiel verfügbar…",
            ["step.voicesNone"] = "Alle Windows-Stimmen waren schon für das Spiel verfügbar.",
            ["step.voiceAdded"] = "Stimme {0} hinzugefügt.",
            ["step.voicesAdded"] = "Windows-Stimmen hinzugefügt: {0}.",
            ["done.ok.heading"] = "Fertig. Die NVDA-Stimme ist bereit.",
            ["done.ok.text"] =
                "Die Stimme SAPI2SR ist installiert, registriert und signiert, und das Spiel kann jede Windows-Stimme dieses Computers nutzen.\n\n" +
                "Nächste Schritte:\n" +
                "1. Starte NVDA, falls es nicht läuft.\n" +
                "2. Starte World of Warcraft.\n" +
                "3. Wähle in den Text-zu-Sprache-Einstellungen des Spiels die Stimme SAPI2SR. " +
                "Alles, was das Spiel mit dieser Stimme spricht, liest jetzt NVDA vor.\n\n" +
                "Das Spiel nummeriert seine Stimmen nach ihrem Platz in der Windows-Liste, neue Stimmen können diese Nummern also verschieben. " +
                "Klingt eine Stimme im Spiel oder in einem Addon anders als vorher, wähle sie neu.",
            ["done.fail.heading"] = "Nicht fertig. Einige Schritte haben nicht geklappt.",
            ["done.fail.text"] = "Diese Probleme sind aufgetreten:\n{0}\n\nBehebe die Ursache und führe das Setup erneut aus.\n\nProtokolldatei: {1}",
            ["done.close"] = "&Schließen",
            ["done.openLog"] = "Protokoll im &Editor öffnen",
            ["done.textAcc"] = "Ergebnis",
            ["err.fileLocked"] = "- Eine Datei ist in Benutzung und konnte nicht ersetzt werden: {0}. Schließe World of Warcraft und andere Programme, die Windows-Stimmen nutzen.",
            ["err.dllMissing"] = "- Stimm-Engine-Datei fehlt: {0}",
            ["err.register"] = "- Registrieren von {0} fehlgeschlagen ({1}).",
            ["err.sign"] = "- Signieren nicht abgeschlossen: {0}",
            ["err.sign.noScript"] = "das Signier-Skript fehlt im Paket.",
            ["err.sign.2"] = "die Stimmdateien haben nicht die erwartete Version.",
            ["err.sign.3"] = "die Signatur konnte nicht erstellt werden.",
            ["err.sign.4"] = "die Signatur konnte nicht überprüft werden.",
            ["err.sign.5"] = "Administratorrechte fehlen.",
            ["err.sign.6"] = "eine Stimm-Engine-Datei fehlt.",
            ["err.sign.7"] = "eine Stimmdatei ist in Benutzung. Schließe World of Warcraft vollständig und führe das Setup erneut aus.",
            ["err.notRegistered"] = "- Die Stimme SAPI2SR ist nicht bei Windows registriert.",
            ["err.voicesBackup"] = "- Die Stimmenliste konnte nicht gesichert werden, deshalb wurden keine Windows-Stimmen hinzugefügt.",
            ["err.voice"] = "- Die Stimme {0} konnte nicht hinzugefügt werden ({1}).",
            ["err.voices"] = "- Hinzufügen der Windows-Stimmen fehlgeschlagen: {0}",
            ["err.unexpected"] = "- Unerwarteter Fehler: {0}",
        };

        private static readonly Dictionary<string, string> Fr = new Dictionary<string, string>
        {
            ["title"] = "Voix NVDA pour WoW",
            ["welcome.heading"] = "Faire de NVDA la voix de World of Warcraft",
            ["welcome.text"] =
                "Ce programme permet à World of Warcraft de parler via NVDA. L'addon Sku n'est pas nécessaire.\n\n" +
                "Ce qu'il fait :\n" +
                "1. Installe la voix passerelle NVDA SAPI2SR dans les dossiers SAPI2SR sous Program Files.\n" +
                "2. L'enregistre comme voix Windows nommée SAPI2SR.\n" +
                "3. Signe la voix sur cet ordinateur. World of Warcraft refuse de charger les voix non signées ; sans cette étape, la voix reste muette dans le jeu. " +
                "Le certificat est créé uniquement sur cet ordinateur et sa clé privée est supprimée juste après la signature ; il ne peut donc jamais rien signer d'autre.\n" +
                "4. Rend les autres voix Windows disponibles pour le jeu. Windows range ses voix plus récentes (par exemple Hortense, Julie et Paul pour le français) dans une liste à part que le jeu ne voit pas ; ce programme copie leurs entrées dans la liste du jeu. " +
                "Les voix elles-mêmes ne sont pas modifiées, et la liste est sauvegardée avant.\n\n" +
                "Avant de commencer :\n" +
                "- Fermez complètement World of Warcraft.\n" +
                "- NVDA doit être lancé pendant que vous jouez.\n\n" +
                "Relancer ce programme répare ou met à jour la voix, et ajoute les voix Windows installées depuis (dans les paramètres Windows, sous Heure et langue).\n\n" +
                "Appuyez sur Installer pour commencer, ou sur Annuler pour quitter sans rien modifier.",
            ["welcome.textAcc"] = "Description",
            ["welcome.install"] = "&Installer",
            ["welcome.cancel"] = "Annuler",
            ["wowRunning"] = "World of Warcraft est encore lancé ({0}).\n\nFermez complètement le jeu, puis choisissez Recommencer. Choisissez Annuler pour quitter.",
            ["progress.heading"] = "Installation de la voix NVDA. Veuillez patienter…",
            ["progress.logAcc"] = "Progression",
            ["step.extract"] = "Décompression des fichiers de la voix…",
            ["step.copy"] = "Copie des fichiers de la voix dans Program Files…",
            ["step.copied"] = "{0} fichiers copiés.",
            ["step.register"] = "Enregistrement de la voix SAPI2SR auprès de Windows…",
            ["step.sign"] = "Signature de la voix pour que World of Warcraft la charge. Cela peut prendre jusqu'à une minute…",
            ["step.check"] = "Vérification de l'installation…",
            ["step.voices"] = "Mise à disposition des autres voix Windows pour le jeu…",
            ["step.voicesNone"] = "Toutes les voix Windows étaient déjà disponibles pour le jeu.",
            ["step.voiceAdded"] = "Voix {0} ajoutée.",
            ["step.voicesAdded"] = "Voix Windows ajoutées : {0}.",
            ["done.ok.heading"] = "Terminé. La voix NVDA est prête.",
            ["done.ok.text"] =
                "La voix SAPI2SR est installée, enregistrée et signée, et le jeu peut utiliser toutes les voix Windows de cet ordinateur.\n\n" +
                "Étapes suivantes :\n" +
                "1. Lancez NVDA s'il n'est pas déjà lancé.\n" +
                "2. Lancez World of Warcraft.\n" +
                "3. Dans les réglages de synthèse vocale du jeu, choisissez la voix SAPI2SR. " +
                "Tout ce que le jeu prononce avec cette voix est désormais lu par NVDA.\n\n" +
                "Le jeu numérote ses voix selon leur place dans la liste de Windows ; ajouter des voix peut donc changer ces numéros. " +
                "Si une voix sonne autrement qu'avant dans le jeu ou dans un addon, choisissez-la de nouveau.",
            ["done.fail.heading"] = "Non terminé. Certaines étapes n'ont pas abouti.",
            ["done.fail.text"] = "Ces problèmes sont survenus :\n{0}\n\nCorrigez la cause et relancez ce programme.\n\nFichier journal : {1}",
            ["done.close"] = "&Fermer",
            ["done.openLog"] = "&Ouvrir le journal dans le Bloc-notes",
            ["done.textAcc"] = "Résultat",
            ["err.fileLocked"] = "- Un fichier est utilisé et n'a pas pu être remplacé : {0}. Fermez World of Warcraft et les autres programmes utilisant des voix Windows.",
            ["err.dllMissing"] = "- Fichier du moteur vocal manquant : {0}",
            ["err.register"] = "- L'enregistrement de {0} a échoué ({1}).",
            ["err.sign"] = "- La signature ne s'est pas terminée : {0}",
            ["err.sign.noScript"] = "le script de signature manque dans le paquet.",
            ["err.sign.2"] = "les fichiers de la voix ne sont pas la version attendue.",
            ["err.sign.3"] = "la création de la signature a échoué.",
            ["err.sign.4"] = "la signature n'a pas pu être vérifiée.",
            ["err.sign.5"] = "les droits administrateur manquent.",
            ["err.sign.6"] = "un fichier du moteur vocal manque.",
            ["err.sign.7"] = "un fichier de la voix est utilisé. Fermez complètement World of Warcraft et relancez ce programme.",
            ["err.notRegistered"] = "- La voix SAPI2SR n'est pas enregistrée auprès de Windows.",
            ["err.voicesBackup"] = "- La liste des voix n'a pas pu être sauvegardée ; aucune voix Windows n'a été ajoutée.",
            ["err.voice"] = "- La voix {0} n'a pas pu être ajoutée ({1}).",
            ["err.voices"] = "- L'ajout des voix Windows a échoué : {0}",
            ["err.unexpected"] = "- Erreur inattendue : {0}",
        };
    }
}
