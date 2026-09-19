using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Win32;

namespace NvdaWowVoice
{
    /// <summary>
    /// The SAPI2SR bridge install, as the full Sku installer does it
    /// (SkuInstaller\Sapi2SrInstaller.cs), but reporting every problem back
    /// instead of degrading silently: here the voice is the ONLY thing the user
    /// came for, so "partly worked" has to read as "not finished".
    ///
    ///   1. extract the embedded payload (copy of the Sku installer's payload);
    ///   2. copy x64 + x86 into Program Files\SAPI2SR;
    ///   3. regsvr32 both engines (writes the COM class + the SAPI voice token);
    ///   4. run sku-nvda-voice-sign.ps1 -Mode Install from the payload, which
    ///      signs the engines with a per-machine throwaway certificate so
    ///      Blizzard's loader gate lets WoW load them;
    ///   5. check the voice token is registered.
    /// </summary>
    internal static class BridgeSetup
    {
        private const string VoiceTokenKey = @"SOFTWARE\Microsoft\Speech\Voices\Tokens\SAPI2SR";

        private static string Dir64 => Path.Combine(
            Environment.GetEnvironmentVariable("ProgramW6432")
                ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "SAPI2SR");

        private static string Dir86 => Path.Combine(
            Environment.GetEnvironmentVariable("ProgramFiles(x86)")
                ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "SAPI2SR");

        // Process names of every WoW client flavour (retail, Classic, Anniversary,
        // PTR/beta builds). A running client holds the engine DLL loaded, which
        // makes both the copy and the signing fail with a file lock.
        private static readonly string[] WowProcessNames =
        {
            "Wow", "WowT", "WowB", "WowClassic", "WowClassicT", "WowClassicB", "Wow-64",
        };

        public static List<string> RunningWowProcesses()
        {
            var found = new List<string>();
            foreach (var name in WowProcessNames)
            {
                try
                {
                    var procs = Process.GetProcessesByName(name);
                    if (procs.Length > 0) found.Add(name + ".exe");
                    foreach (var p in procs) p.Dispose();
                }
                catch { }
            }
            return found;
        }

        /// <summary>Runs every step. <paramref name="report"/> gets one line per
        /// milestone (shown and spoken). Returns the problems; empty = success.</summary>
        public static List<string> Run(Action<string> report)
        {
            var problems = new List<string>();
            string staging = Path.Combine(Path.GetTempPath(), "NvdaWowVoiceSetup_payload");
            try
            {
                report(Strings.Get("step.extract"));
                ExtractPayload(staging);

                report(Strings.Get("step.copy"));
                int copied = CopyTree(Path.Combine(staging, "x64"), Dir64, problems)
                           + CopyTree(Path.Combine(staging, "x86"), Dir86, problems);
                report(Strings.Format("step.copied", copied));

                report(Strings.Get("step.register"));
                string sys32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
                string wow64 = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
                Regsvr32(Path.Combine(sys32, "regsvr32.exe"), Path.Combine(Dir64, "sapi2sr_engine.dll"), problems);
                Regsvr32(Path.Combine(wow64, "regsvr32.exe"), Path.Combine(Dir86, "sapi2sr_engine.dll"), problems);

                report(Strings.Get("step.sign"));
                Sign(Path.Combine(staging, "sku-nvda-voice-sign.ps1"), problems);

                report(Strings.Get("step.check"));
                if (!VoiceRegistered())
                    problems.Add(Strings.Get("err.notRegistered"));
            }
            catch (Exception ex)
            {
                Log.Error("Setup failed", ex);
                problems.Add(Strings.Format("err.unexpected", ex.Message));
            }
            finally
            {
                try { if (Directory.Exists(staging)) Directory.Delete(staging, true); } catch { }
            }

            foreach (var p in problems) Log.Warn("Problem: " + p);
            return problems;
        }

        private static void ExtractPayload(string staging)
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, true);
            Directory.CreateDirectory(staging);

            var asm = Assembly.GetExecutingAssembly();
            string res = asm.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("sapi2sr-payload.zip", StringComparison.OrdinalIgnoreCase));
            if (res == null)
                throw new InvalidOperationException("Embedded SAPI2SR payload not found.");

            using (var s = asm.GetManifestResourceStream(res))
            using (var zip = new ZipArchive(s, ZipArchiveMode.Read))
                zip.ExtractToDirectory(staging);
            Log.Info("Payload extracted to " + staging);
        }

        private static int CopyTree(string src, string dst, List<string> problems)
        {
            if (!Directory.Exists(src)) return 0;
            int n = 0;
            Directory.CreateDirectory(dst);
            foreach (var file in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
            {
                string rel = file.Substring(src.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string target = Path.Combine(dst, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                try { File.Copy(file, target, true); n++; }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    Log.Warn($"Could not write {target}: {ex.Message}");
                    problems.Add(Strings.Format("err.fileLocked", target));
                }
            }
            Log.Info($"Copied {n} file(s) to {dst}");
            return n;
        }

        private static void Regsvr32(string regsvr32, string dll, List<string> problems)
        {
            if (!File.Exists(dll))
            {
                Log.Warn("Engine DLL missing: " + dll);
                problems.Add(Strings.Format("err.dllMissing", dll));
                return;
            }
            var psi = new ProcessStartInfo(regsvr32, $"/s \"{dll}\"") { UseShellExecute = false, CreateNoWindow = true };
            using (var p = Process.Start(psi))
            {
                if (!p.WaitForExit(30000))
                {
                    Log.Warn("regsvr32 timed out for " + dll);
                    problems.Add(Strings.Format("err.register", dll, "timeout"));
                    return;
                }
                if (p.ExitCode != 0)
                {
                    Log.Warn($"regsvr32 exit {p.ExitCode} for {dll}");
                    problems.Add(Strings.Format("err.register", dll, p.ExitCode));
                }
                else Log.Info("Registered " + dll);
            }
        }

        private static void Sign(string script, List<string> problems)
        {
            if (!File.Exists(script))
            {
                problems.Add(Strings.Format("err.sign", Strings.Get("err.sign.noScript")));
                return;
            }

            var psi = new ProcessStartInfo("powershell.exe",
                $"-NoProfile -ExecutionPolicy Bypass -File \"{script}\" -Mode Install")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
            };
            using (var p = new Process { StartInfo = psi })
            {
                p.OutputDataReceived += (s, e) => { if (e.Data != null) Log.Info("sign: " + e.Data); };
                p.ErrorDataReceived += (s, e) => { if (e.Data != null) Log.Warn("sign: " + e.Data); };
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                if (!p.WaitForExit(180000))
                {
                    try { p.Kill(); } catch { }
                    problems.Add(Strings.Format("err.sign", "timeout"));
                    return;
                }
                p.WaitForExit(); // flush the async readers
                Log.Info("Signing script exit code " + p.ExitCode);
                if (p.ExitCode == 0) return;

                // Exit codes are documented in the script's header.
                string key;
                switch (p.ExitCode)
                {
                    case 2: key = "err.sign.2"; break;
                    case 3: key = "err.sign.3"; break;
                    case 4: key = "err.sign.4"; break;
                    case 5: key = "err.sign.5"; break;
                    case 6: key = "err.sign.6"; break;
                    case 7: key = "err.sign.7"; break;
                    default: key = null; break;
                }
                problems.Add(Strings.Format("err.sign", key != null ? Strings.Get(key) : "code " + p.ExitCode));
            }
        }

        private static bool VoiceRegistered()
        {
            foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                    using (var k = hklm.OpenSubKey(VoiceTokenKey))
                        if (k == null) { Log.Warn($"Voice token missing ({view})"); return false; }
                }
                catch (Exception ex) { Log.Warn($"Registry check failed ({view}): {ex.Message}"); return false; }
            }
            return true;
        }
    }
}
