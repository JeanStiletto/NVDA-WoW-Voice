using System;
using System.Reflection;
using System.Windows.Forms;

namespace NvdaWowVoice
{
    internal static class Program
    {
        public static readonly string Version =
            Assembly.GetExecutingAssembly().GetName().Version.ToString(2);

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Strings.Init();
            Log.Info($"=== NVDA Voice for WoW {Version} starting ===");

            var welcome = new WelcomeForm();
            Application.Run(welcome);
            if (!welcome.Start) { Log.Info("Cancelled at the welcome screen."); return; }

            var progress = new ProgressForm();
            Application.Run(progress);

            Log.Info(progress.Problems.Count == 0 ? "Finished OK." : $"Finished with {progress.Problems.Count} problem(s).");
            Application.Run(new DoneForm(progress.Problems));
        }
    }
}
