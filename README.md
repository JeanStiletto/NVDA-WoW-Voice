# NVDA Voice for WoW

Makes the NVDA screen reader the voice of World of Warcraft, and makes every
other Windows voice on your computer available to the game.

World of Warcraft has built-in text to speech and can use any Windows SAPI voice.
The SAPI2SR bridge voice forwards everything spoken with it to NVDA. Since late
2025 the game refuses to load **unsigned** voice engines, so the bridge voice
shows up in the voice list but stays silent. This setup installs the bridge and
signs it on your own computer so the game loads it again.

Windows also keeps its newer voices (the Narrator voices, for example Katja and
Stefan for German) in a separate list the game cannot see, so most computers
offer the game only one or two voices. The setup copies those voices' entries
into the game's list, so the game can use all of them.

You do not need the Sku addon for this. (Sku's own installer does the same bridge step.)

## Download

**[Download NvdaWowVoiceSetup.exe (latest version)](https://github.com/JeanStiletto/NVDA-WoW-Voice/releases/latest/download/NvdaWowVoiceSetup.exe)**

Run it, accept the administrator prompt, read the short explanation and press
Install. The setup speaks every step through your screen reader and tells you
when it is finished. English, German and French, chosen from your Windows
language.

## Before you start

- Close World of Warcraft completely. The setup checks this and asks you to
  close it if it is still running.
- NVDA must be running while you play.

## After the setup

1. Start NVDA.
2. Start World of Warcraft.
3. In the game's text to speech settings, choose the voice **SAPI2SR**.

Running the setup again repairs or updates the voice, and adds Windows voices you
installed since (Windows settings, Time and Language, Speech, Add voices).

The game numbers its voices by their place in the Windows list, so added voices
can change those numbers. If a voice in the game or in an addon sounds different
than before, choose it again.

## What exactly it does

1. Copies the SAPI2SR voice files to `C:\Program Files\SAPI2SR` and
   `C:\Program Files (x86)\SAPI2SR`.
2. Registers the voice with Windows (`regsvr32`), under the name SAPI2SR.
3. Runs the signing script (`signing/sku-nvda-voice-sign.ps1`, also bundled in
   the exe) which:
   - creates a **fresh certificate on your computer**, valid for code signing only;
   - signs only the two voice engine DLLs and the speech-history DLL (below),
     and only if their hash matches the versions pinned in the script;
   - **deletes the private key right after signing**, so the certificate can
     never sign anything else;
   - writes a receipt and log to `C:\ProgramData\Sku\`.
4. Checks that the voice is registered.
5. Makes the other Windows voices available to the game: copies every voice
   entry from the Windows OneCore voice list
   (`HKLM\SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens`) that the SAPI
   list (`HKLM\SOFTWARE\Microsoft\Speech\Voices\Tokens`, 64-bit, the one the
   game reads) lacks. Only the entries are copied; the voices stay where Windows
   installed them, and their engine is Microsoft's own, signed, so the game
   loads it. Before the first change the SAPI list is backed up to
   `C:\ProgramData\Sku\sapi-voices-<date>.reg`.

Log file of the setup: `C:\ProgramData\Sku\nvda-wow-voice-setup.log`.

To take the added Windows voices out again, import the backup
(`reg import C:\ProgramData\Sku\sapi-voices-<date>.reg` in an administrator
command prompt restores the list as it was), or delete the copied `MSTTS_...`
entries from the SAPI list.

To undo the signing, run the script with `-Mode Uninstall` in an administrator
PowerShell. `-Mode Verify` shows a read-only health report.

## Speech history (since 1.2)

The 64-bit voice comes with a speech history: `C:\Program Files\SAPI2SR\nvdaControllerClient.dll`
passes every call on to NV Access's original (kept next to it as
`nvdaControllerClient.nvaccess.dll`, still signed by NV Access), and can keep
the last 64 texts a program spoke, so another program can read them back.

- Nothing is recorded unless a program asks for it, and then only the texts
  of the one program it asks for, for as long as it keeps asking.
- Only programs of the same Windows user can read it; it lives in memory
  only and is gone when the last program using the voice closes.
- Today the one program that asks is the WoW Login Tool (part of Sku), and
  only while it drives WoW Forever's login and character screens: the game
  narrates them, but stays silent when an item is narrated a second time, and
  the tool then repeats what the game said the first time. Nothing is recorded
  while you play.
- Switched off completely with `%LOCALAPPDATA%\SAPI2SR\SpeechHistory.ini`
  containing `[SpeechHistory]` and `Publish=0`.

Source and the reading rules:
[speech-history](https://github.com/Sku75/Sku-WoW-Addon-TBC/tree/main/dev/rework-docs/nvda-voice-signing/speech-history)
in the Sku repository.

## Risk

This is an unofficial workaround. It does not touch the game's files or memory,
it only lets the game load a normal Windows voice again. Still, Blizzard did not
approve it, a future game patch may silence the voice again, and you use it at
your own risk.

## Credits

- **SAPI2SR** — the NVDA SAPI bridge voice, third-party software. The 64-bit
  engine bundled here is patched so it works with the current WoW text to speech
  (skips WoW's boundary bookmarks, and supports interrupting speech).
- **NVDA Controller Client** — NV Access. The speech-history client in front
  of it is Sku's.
- Part of the [Sku](https://github.com/Sku75/Sku-WoW-Addon-TBC) project.

## Building

.NET SDK with the .NET Framework 4.8 targeting pack:

```
dotnet build NvdaWowVoice.csproj -c Release
```

Releasing (maintainers): bump `<Version>` in `NvdaWowVoice.csproj`, then
`.\release.ps1 -Notes "what changed"`.

---

# NVDA-Stimme für WoW (Deutsch)

Macht NVDA zur Stimme von World of Warcraft und macht alle anderen
Windows-Stimmen deines Computers für das Spiel verfügbar (Windows führt seine
neueren Stimmen, für Deutsch zum Beispiel Katja und Stefan, in einer eigenen
Liste, die das Spiel nicht sieht). Das Setup installiert die
NVDA-Brückenstimme SAPI2SR und signiert sie auf deinem Computer, denn das Spiel
lädt seit Ende 2025 keine unsignierten Stimmen mehr. Das Sku-Addon brauchst du
dafür nicht.

**[NvdaWowVoiceSetup.exe herunterladen (neueste Version)](https://github.com/JeanStiletto/NVDA-WoW-Voice/releases/latest/download/NvdaWowVoiceSetup.exe)**

1. World of Warcraft vollständig schließen.
2. Setup starten, Administrator-Abfrage bestätigen, Installieren drücken. Jeder
   Schritt wird vorgelesen.
3. NVDA starten, WoW starten und in den Text-zu-Sprache-Einstellungen des Spiels
   die Stimme **SAPI2SR** wählen. Dort stehen jetzt auch die übrigen
   Windows-Stimmen. Neue Stimmen können die Nummern der Stimmen im Spiel
   verschieben: Klingt eine Stimme anders als vorher, wähle sie neu.

Inoffizieller Workaround, Nutzung auf eigenes Risiko.
