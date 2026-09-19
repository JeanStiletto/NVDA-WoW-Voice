using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Automation;

namespace NvdaWowVoice
{
    /// <summary>
    /// Appends to %ProgramData%\Sku\nvda-wow-voice-setup.log - next to the
    /// signing script's own nvda-voice-signing.log, so one folder holds the whole
    /// story when someone reports a silent voice. Never throws.
    /// </summary>
    internal static class Log
    {
        public static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Sku", "nvda-wow-voice-setup.log");

        private static readonly object _lock = new object();

        public static void Info(string msg) => Write("INFO", msg);
        public static void Warn(string msg) => Write("WARN", msg);
        public static void Error(string msg, Exception ex) => Write("ERROR", $"{msg}: {ex}");

        private static void Write(string level, string msg)
        {
            try
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    File.AppendAllText(FilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {level}: {msg}{Environment.NewLine}");
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Speaks a line through the screen reader via a UIA notification (same
    /// technique as SkuInstaller.ScreenReaderAnnouncer): changing a label's text
    /// alone tells NVDA/JAWS nothing.
    /// </summary>
    internal static class Announcer
    {
        public static void Say(Control source, string message)
        {
            if (source == null || string.IsNullOrEmpty(message)) return;
            try
            {
                source.AccessibilityObject?.RaiseAutomationNotification(
                    AutomationNotificationKind.ActionCompleted,
                    AutomationNotificationProcessing.All,
                    message);
            }
            catch (Exception ex) { Log.Warn("Announce failed: " + ex.Message); }
        }
    }
}
