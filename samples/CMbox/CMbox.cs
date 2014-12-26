using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Codice.CmdRunner;

namespace CmdRunnerExamples
{
    public class CMbox : Form
    {
        [STAThread]
        public static void Main()
        {
            Application.Run(new CMbox());
        }

        public CMbox()
        {
            ConfigureMenu();
            string server = Configuration.ServerName;
            mSampleRep = SampleHelper.GenerateEmptyRepository(server);

            if (Configuration.IsSimulation)
                CMboxHelper.StartSimulation();
        }

        private void ConfigureMenu()
        {
            mTrayMenu = new ContextMenu();
            mTrayMenu.MenuItems.Add("Exit", OnExit);

            mTrayIcon = new NotifyIcon();
            mTrayIcon.Text = "Notifier";
            mTrayIcon.Icon = new Icon(SystemIcons.WinLogo, 40, 40);
            mTrayIcon.ContextMenu = mTrayMenu;
            mTrayIcon.Visible = true;

            Timer mTimer = new Timer();
            mTimer.Interval = TIMER_INTERVAL;
            mTimer.Tick += new EventHandler(CheckinUpdates);
            mTimer.Start();
        }

        private void CheckinUpdates(object sender, EventArgs e)
        {
            List<Change> mChanges = GetChanges();
            if (mChanges.Count == 0)
                return;

            StringBuilder builder = new StringBuilder();
            foreach (var item in mChanges)
                builder.Append(string.Format("{0} {1}\n",
                    item.ChangeType, item.Name));

            foreach (var item in mChanges)
            {
                if (item.ChangeType == ChangeType.Added)
                    CmdRunner.ExecuteCommandWithStringResult(
                        string.Format("cm add {0}", item.Name),
                        SampleHelper.GetWorkspace());

                if (item.ChangeType == ChangeType.Deleted)
                    CmdRunner.ExecuteCommandWithStringResult(
                        string.Format("cm rm {0}", item.Name),
                        SampleHelper.GetWorkspace());
            }

            CmdRunner.ExecuteCommandWithStringResult("cm ci ",
                SampleHelper.GetWorkspace());

        mTrayIcon.ShowBalloonTip(3, string.Format("{0} new changes saved", mChanges.Count),
            string.Format("The following changes have been checked in.\n{0}",
            builder.ToString()),
            ToolTipIcon.Info);
        }

        private List<Change> GetChanges()
        {
            List<Change> changes = new List<Change>();
            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                    string.Format("cm status --all --machinereadable"),
                    SampleHelper.GetWorkspace());

            ArrayList results = SampleHelper.GetListResults(cmdResult, true);
            for (int i = 1; i < results.Count; i++)
                changes.Add(new Change((string)results[i]));

            return changes;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            base.OnLoad(e);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
                mTrayIcon.Dispose();

            base.Dispose(isDisposing);
        }

        private void NotifyChanges(string title, string description)
        {
            mTrayIcon.ShowBalloonTip(BALLOON_TIMEOUT, title,
                description, ToolTipIcon.Info);
        }

        private void OnExit(object sender, EventArgs e)
        {
            if (Configuration.IsSimulation)
                CMboxHelper.StopSimulation();

            SampleHelper.RemoveSampleRepository(mSampleRep);
            mTrayIcon.Visible = false;
            Application.Exit();
        }

        private NotifyIcon mTrayIcon;
        private ContextMenu mTrayMenu;
        private string mSampleRep;
        private static int BALLOON_TIMEOUT = 3;
        private static int TIMER_INTERVAL = 10000;
    }
}