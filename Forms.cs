using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NvdaWowVoice
{
    /// <summary>
    /// Shared shape of all three windows: a heading, a read-only multiline text
    /// box (reviewable line by line with the arrow keys, unlike a Label which a
    /// screen reader reads as one blob), and a row of buttons. The whole text is
    /// spoken once when the window appears, so reviewing it is optional.
    /// </summary>
    internal class StepForm : Form
    {
        protected readonly Label Heading;
        protected readonly TextBox Body;
        private readonly FlowLayoutPanel _buttons;

        public StepForm(string heading, string body, string bodyAccName)
        {
            Text = Strings.Get("title") + " " + Program.Version;
            ClientSize = new Size(620, 440);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);

            Heading = new Label
            {
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                Location = new Point(20, 16),
                Size = new Size(580, 32),
                Text = heading,
            };
            Body = new TextBox
            {
                Location = new Point(20, 56),
                Size = new Size(580, 310),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Text = Lines(body),
                AccessibleName = bodyAccName,
                TabIndex = 0,
            };
            _buttons = new FlowLayoutPanel
            {
                Location = new Point(20, 380),
                Size = new Size(580, 44),
                FlowDirection = FlowDirection.LeftToRight,
                TabIndex = 1,
            };
            Controls.AddRange(new Control[] { Heading, Body, _buttons });
        }

        protected Button AddButton(string text, EventHandler onClick)
        {
            var b = new Button { Text = text, AutoSize = true, MinimumSize = new Size(140, 36) };
            b.Click += onClick;
            _buttons.Controls.Add(b);
            return b;
        }

        protected static string Lines(string s) =>
            (s ?? "").Replace("\r\n", "\n").Replace("\n", Environment.NewLine);

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Announcer.Say(this, Heading.Text + ". " + Body.Text);
        }
    }

    /// <summary>What it does + Install / Cancel.</summary>
    internal sealed class WelcomeForm : StepForm
    {
        public bool Start { get; private set; }

        public WelcomeForm()
            : base(Strings.Get("welcome.heading"), Strings.Get("welcome.text"), Strings.Get("welcome.textAcc"))
        {
            var install = AddButton(Strings.Get("welcome.install"), (s, e) => TryStart());
            var cancel = AddButton(Strings.Get("welcome.cancel"), (s, e) => Close());
            AcceptButton = install;
            CancelButton = cancel;
            ActiveControl = install;
        }

        private void TryStart()
        {
            // WoW holds the engine DLL while running: copying and signing would
            // fail on a file lock. Ask until it is closed or the user gives up.
            while (true)
            {
                var running = BridgeSetup.RunningWowProcesses();
                if (running.Count == 0) break;
                Log.Info("WoW running: " + string.Join(", ", running));
                var r = MessageBox.Show(this, Strings.Format("wowRunning", string.Join(", ", running)),
                    Strings.Get("title"), MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                if (r != DialogResult.Retry) return;
            }
            Start = true;
            Close();
        }
    }

    /// <summary>Runs the setup on a worker thread; every step is appended to
    /// the text box and spoken. Cannot be closed while running.</summary>
    internal sealed class ProgressForm : StepForm
    {
        public List<string> Problems { get; private set; } = new List<string>();
        private bool _running;

        public ProgressForm()
            : base(Strings.Get("progress.heading"), "", Strings.Get("progress.logAcc"))
        {
            ControlBox = false;
            ActiveControl = Body;
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _running = true;
            Problems = await Task.Run(() => BridgeSetup.Run(Report));
            _running = false;
            Close();
        }

        private void Report(string line)
        {
            Log.Info(line);
            BeginInvoke((Action)(() =>
            {
                Body.AppendText((Body.TextLength > 0 ? Environment.NewLine : "") + line);
                Announcer.Say(this, line);
            }));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_running) e.Cancel = true;
            base.OnFormClosing(e);
        }
    }

    /// <summary>The confirmation at the end: success with next steps, or the
    /// problem list and where the log is.</summary>
    internal sealed class DoneForm : StepForm
    {
        public DoneForm(List<string> problems)
            : base(problems.Count == 0 ? Strings.Get("done.ok.heading") : Strings.Get("done.fail.heading"),
                   problems.Count == 0
                       ? Strings.Get("done.ok.text")
                       : Strings.Format("done.fail.text", string.Join("\n", problems), Log.FilePath),
                   Strings.Get("done.textAcc"))
        {
            var close = AddButton(Strings.Get("done.close"), (s, e) => Close());
            AddButton(Strings.Get("done.openLog"), (s, e) =>
            {
                try { Process.Start("notepad.exe", "\"" + Log.FilePath + "\""); }
                catch (Exception ex) { Log.Warn("Could not open log: " + ex.Message); }
            });
            AcceptButton = close;
            CancelButton = close;
            ActiveControl = close;
        }
    }
}
