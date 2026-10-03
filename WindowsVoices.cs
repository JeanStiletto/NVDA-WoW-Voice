using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace NvdaWowVoice
{
    /// <summary>
    /// Makes the Windows voices the game cannot see available to it.
    ///
    /// World of Warcraft speaks with SAPI 5 voices: the voice tokens under
    /// HKLM\SOFTWARE\Microsoft\Speech\Voices\Tokens. Windows keeps its newer
    /// voices (the Narrator voices, for example Katja and Stefan for German,
    /// added in Settings, Time and Language, Speech) in a separate list,
    /// Speech_OneCore\Voices\Tokens, so the game does not list them. Their
    /// engine is a regular, Microsoft-signed SAPI engine, so copying a token
    /// into the SAPI list is all it takes: the voice files stay where Windows
    /// installed them. Only the 64-bit list, which the game reads.
    ///
    ///   1. find the OneCore tokens missing from the SAPI list (by token name);
    ///   2. back up the SAPI list (reg export, next to the log);
    ///   3. copy each missing token with its values and Attributes subkey.
    ///
    /// Running the setup again copies voices added since. Undo: import the
    /// backup, or delete the copied MSTTS_* tokens from the SAPI list.
    /// </summary>
    internal static class WindowsVoices
    {
        private const string SapiTokens = @"SOFTWARE\Microsoft\Speech\Voices\Tokens";
        private const string OneCoreTokens = @"SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens";

        private static string BackupDir => Path.GetDirectoryName(Log.FilePath);

        /// <summary>Runs every step. <paramref name="report"/> gets one line per
        /// milestone (shown and spoken). Returns the problems; empty = success.</summary>
        public static List<string> Run(Action<string> report)
        {
            var problems = new List<string>();
            report(Strings.Get("step.voices"));
            try
            {
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var oneCore = hklm.OpenSubKey(OneCoreTokens))
                using (var sapi = hklm.CreateSubKey(SapiTokens))
                {
                    var present = new HashSet<string>(sapi.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
                    var missing = oneCore == null
                        ? new List<string>()
                        : oneCore.GetSubKeyNames().Where(name => !present.Contains(name)).ToList();
                    Log.Info($"Windows voices: {present.Count} SAPI token(s), {missing.Count} OneCore voice(s) missing from it");
                    if (missing.Count == 0)
                    {
                        report(Strings.Get("step.voicesNone"));
                        return problems;
                    }

                    if (!Backup())
                    {
                        // No way back without the backup: change nothing.
                        problems.Add(Strings.Get("err.voicesBackup"));
                        return problems;
                    }

                    int added = 0;
                    foreach (var name in missing)
                    {
                        using (var source = oneCore.OpenSubKey(name))
                        {
                            string label = source?.GetValue("") as string ?? name;
                            try
                            {
                                using (var target = sapi.CreateSubKey(name))
                                    CopyKey(source, target);
                                Log.Info($"Copied voice token {name} ({label})");
                                report(Strings.Format("step.voiceAdded", label));
                                added++;
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"Copying voice token {name} failed", ex);
                                // A half-copied token would list a voice that cannot speak.
                                try { sapi.DeleteSubKeyTree(name, false); } catch { }
                                problems.Add(Strings.Format("err.voice", label, ex.Message));
                            }
                        }
                    }
                    if (added > 0)
                        report(Strings.Format("step.voicesAdded", added));
                }
            }
            catch (Exception ex)
            {
                Log.Error("Making the Windows voices available failed", ex);
                problems.Add(Strings.Format("err.voices", ex.Message));
            }
            return problems;
        }

        /// <summary>Values (kinds and unexpanded %variables% kept) and subkeys,
        /// recursively.</summary>
        private static void CopyKey(RegistryKey source, RegistryKey target)
        {
            foreach (var valueName in source.GetValueNames())
            {
                target.SetValue(valueName,
                    source.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames),
                    source.GetValueKind(valueName));
            }
            foreach (var subName in source.GetSubKeyNames())
            {
                using (var sourceSub = source.OpenSubKey(subName))
                using (var targetSub = target.CreateSubKey(subName))
                    CopyKey(sourceSub, targetSub);
            }
        }

        private static bool Backup()
        {
            try
            {
                Directory.CreateDirectory(BackupDir);
                string file = Path.Combine(BackupDir, $"sapi-voices-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.reg");
                string reg = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "reg.exe");
                var psi = new ProcessStartInfo(reg, $"export \"HKLM\\{SapiTokens}\" \"{file}\" /y /reg:64")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using (var p = Process.Start(psi))
                {
                    if (!p.WaitForExit(30000) || p.ExitCode != 0 || !File.Exists(file))
                    {
                        Log.Warn("Voice list backup failed");
                        return false;
                    }
                }
                Log.Info("Voice list backed up to " + file);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Voice list backup failed", ex);
                return false;
            }
        }
    }
}
