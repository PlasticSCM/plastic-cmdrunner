using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;

using Codice.CmdRunner;
using System.Threading;
using System.Text;

namespace CmdRunnerExamples
{
    public class Notifier : Form
    {
        [STAThread]
        public static void Main()
        {
            Application.Run(new Notifier());
        }

        public Notifier()
        {
            ConfigureMenu();

            string server = Configuration.ServerName;
            mSampleRep = SampleHelper.GenerateRepositoryWithData(server);
            mSecondRep = SampleHelper.GenerateEmptyRepository(server);

            if (Configuration.IsSimulation)
                NotifierHelper.StartSimulation(mSampleRep);
        }

        private void ConfigureMenu()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "Notifier";
            trayIcon.Icon = new Icon(SystemIcons.WinLogo, 40, 40);
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
            trayIcon.BalloonTipClicked += new EventHandler(trayIcon_BalloonTipClicked);

            mTimer = new System.Windows.Forms.Timer();
            mTimer.Interval = 20000;
            mTimer.Tick += new EventHandler(GetUpdates);
            mTimer.Start();
        }

        void GetUpdates(object sender, EventArgs e)
        {
            mBranchesToSync = GetBranchesToSync();
            if (mBranchesToSync == null)
                return;

            StringBuilder builder = new StringBuilder();
            foreach (var item in mBranchesToSync)
                builder.Append(item.Name)
                    .Append("@")
                    .Append(mSampleRep)
                    .Append("\n");

            trayIcon.ShowBalloonTip(3, "New changes found",
                string.Format("The following branches have changes.\n{0} Click on the balloon to replicate.",
                builder.ToString()),
                ToolTipIcon.Info);
        }

        List<Branch> GetBranchesToSync()
        {
            if (mSampleRep == null || mSecondRep == null)
                return null;

            List<Branch> srcBranches = GetBranchesFromRepo(mSampleRep);
            List<Branch> dstBranches = GetBranchesFromRepo(mSecondRep);
            List<Branch> newBranches = new List<Branch>();

            foreach (Branch item in srcBranches)
            {
                if (!dstBranches.Contains(item))
                {
                    newBranches.Add(item);
                    continue;
                }
                
                if (HasChangesInBranch(item))
                    newBranches.Add(item);
            }

            return newBranches;
        }

        private bool HasChangesInBranch(Branch branch)
        {
            string srcResult = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm find changeset where branch = 'br:{0}' on repositories '{1}' --format={{id}} --nototal",
                branch.Name, mSampleRep),
                Environment.CurrentDirectory);

            ArrayList srcResults = SampleHelper.GetListResults(srcResult, true);

            string dstResult = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm find changeset where branch = 'br:{0}' on repositories '{1}' --format={{id}} --nototal",
                branch.Name, mSecondRep),
                Environment.CurrentDirectory);

            ArrayList dstResults = SampleHelper.GetListResults(dstResult, true);

            return srcResults.Count != dstResults.Count;
        }

        List<Branch> GetBranchesFromRepo(string repository)
        {
            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm find branch on repositories '{0}' --format={{id}}#{{name}} --nototal", repository),
                Environment.CurrentDirectory);

            ArrayList results = SampleHelper.GetListResults(cmdResult, true);
            return GetBranchListFromCmdResult(results);
        }

        void trayIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            if (mBranchesToSync == null)
                return;

            mTimer.Stop();

            trayIcon.ShowBalloonTip(1, "Replicating...",
                string.Format("Replicating {0} branches, please wait", mBranchesToSync.Count),
                ToolTipIcon.Info);

            List<ReplicationResult> resultList = new List<ReplicationResult>();
            foreach (Branch branch in mBranchesToSync)
                resultList.Add(Replicate(branch, mSampleRep, mSecondRep));

            trayIcon.ShowBalloonTip(1, "Replication complete.",
                string.Format("{0} branches replicated", resultList.Count),
                ToolTipIcon.Info);

            mBranchesToSync = null;

            mTimer.Start();
        }

        static List<Branch> GetBranchListFromCmdResult(ArrayList results)
        {
            List<Branch> branches = new List<Branch>();
            foreach (string branch in results)
                branches.Add(new Branch(branch));

            return branches;
        }

        private static ReplicationResult Replicate(Branch branch, string repository,
            string secondRepository)
        {
            Console.WriteLine("THREAD:{0} Replicating branch {1}",
                Thread.CurrentThread.ManagedThreadId, branch.Name);

            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
            string.Format("cm replicate \"br:{0}@{1}\" \"rep:{2}\"",
                branch.Name, repository, secondRepository),
            Environment.CurrentDirectory);

            return new ReplicationResult(SampleHelper.GetListResults(cmdResult, true), branch);
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
                trayIcon.Dispose();

            base.Dispose(isDisposing);
        }

        private void NotifyChanges(string title, string description)
        {
            trayIcon.ShowBalloonTip(3, title, description, ToolTipIcon.Info);
        }

        private void OnExit(object sender, EventArgs e)
        {
            NotifierHelper.StopSimulation();
            SampleHelper.RemoveSampleRepository(mSampleRep);
            SampleHelper.RemoveSampleRepository(mSecondRep);
            Application.Exit();
        }

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private string mSampleRep;
        private string mSecondRep;
        private List<Branch> mBranchesToSync;
        private System.Windows.Forms.Timer mTimer;
    }
}