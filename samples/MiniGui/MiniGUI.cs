using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Codice.CmdRunner;
using System.IO;

namespace CmdRunnerExamples
{
    public class MiniGUI : Form
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MiniGUI());
        }

        public MiniGUI()
        {
            InitializeComponent();
            string server = Configuration.ServerName;
            mSampleRep = SampleHelper.GenerateEmptyRepository(server);

            if (Configuration.IsSimulation)
                CMboxHelper.StartSimulation();
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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (Configuration.IsSimulation)
                CMboxHelper.StopSimulation();

            SampleHelper.RemoveSampleRepository(mSampleRep);
            Application.Exit();
        }

        private void ShowChangeset(object sender, EventArgs e)
        {
            Changeset selected = (Changeset)changesetList.SelectedItem;
            if (selected == null)
                return;

            commentTextBox.Text = selected.Comment;
            changesListBox.Items.Clear();
            foreach (var item in selected.Changes)
                changesListBox.Items.Add(item);
        }

        private void AddToItemsToCheckin(object sender, EventArgs e)
        {
            Change item = (Change)itemsNotAdded.SelectedItem;
            if (item == null)
                return;

            itemsToCommit.Items.Add(item);
            itemsNotAdded.Items.Remove(item);
        }

        private void Checkin(object sender, EventArgs e)
        {
            foreach (Change item in itemsToCommit.Items)
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

            string result = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm ci -c=\"{0}\"", textBox1.Text),
                SampleHelper.GetWorkspace());

            MessageBox.Show(result);

            textBox1.Text = string.Empty;
            Update(sender, e);
        }

        private void Update(object sender, EventArgs e)
        {
            itemsToCommit.Items.Clear();
            itemsNotAdded.Items.Clear();

            List<Change> mChanges = GetChanges();
            if (mChanges.Count == 0)
                return;

            foreach (var item in mChanges)
            {
                if ((item.ChangeType == ChangeType.Added)
                    || (item.ChangeType == ChangeType.Deleted))
                {
                    itemsNotAdded.Items.Add(item);
                    continue;
                }

                itemsToCommit.Items.Add(item);
            }
        }

        private void RefreshChangesetList(object sender, EventArgs e)
        {
            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                "cm find changeset --nototal --format=\"{changesetid}#{date}#{comment}\"",
                SampleHelper.GetWorkspace());

            ArrayList results = SampleHelper.GetListResults(cmdResult, true);

            changesetList.Items.Clear();
            foreach (string item in results)
            {
                Changeset cset = new Changeset(item);
                cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                    string.Format("cm log {0} --csFormat=\"{{items}}\" --itemFormat=\"{{path}}#{{fullstatus}}#{{newline}}\"", cset.Id),
                    SampleHelper.GetWorkspace());

                results = SampleHelper.GetListResults(cmdResult, true);
                foreach (string changedItem in results)
                    cset.Changes.Add(new Item(changedItem));

                changesetList.Items.Add(cset);
            }
        }

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.itemsToCommit = new System.Windows.Forms.ListBox();
            this.itemsNotAdded = new System.Windows.Forms.ListBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.commentTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.changesListBox = new System.Windows.Forms.ListBox();
            this.changesetList = new System.Windows.Forms.ListBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Included files";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 189);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(166, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Not included (double click to add)";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(75, 41);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(437, 20);
            this.textBox1.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Comment:";
            // 
            // itemsToCommit
            // 
            this.itemsToCommit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.itemsToCommit.FormattingEnabled = true;
            this.itemsToCommit.Location = new System.Drawing.Point(9, 80);
            this.itemsToCommit.Name = "itemsToCommit";
            this.itemsToCommit.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.itemsToCommit.Size = new System.Drawing.Size(498, 95);
            this.itemsToCommit.TabIndex = 9;
            // 
            // itemsNotAdded
            // 
            this.itemsNotAdded.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.itemsNotAdded.FormattingEnabled = true;
            this.itemsNotAdded.Location = new System.Drawing.Point(9, 216);
            this.itemsNotAdded.Name = "itemsNotAdded";
            this.itemsNotAdded.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.itemsNotAdded.Size = new System.Drawing.Size(498, 147);
            this.itemsNotAdded.TabIndex = 10;
            this.itemsNotAdded.DoubleClick += new System.EventHandler(this.AddToItemsToCheckin);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(523, 414);
            this.tabControl1.TabIndex = 11;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button3);
            this.tabPage1.Controls.Add(this.button2);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.itemsNotAdded);
            this.tabPage1.Controls.Add(this.itemsToCommit);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.textBox1);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(515, 388);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Pending changes";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(87, 6);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 12;
            this.button3.Text = "Refresh";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Update);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 11;
            this.button2.Text = "Checkin";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Checkin);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.button1);
            this.tabPage2.Controls.Add(this.commentTextBox);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.changesListBox);
            this.tabPage2.Controls.Add(this.changesetList);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(515, 388);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Changesets";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Refresh";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.RefreshChangesetList);
            // 
            // commentTextBox
            // 
            this.commentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commentTextBox.Location = new System.Drawing.Point(198, 58);
            this.commentTextBox.Multiline = true;
            this.commentTextBox.Name = "commentTextBox";
            this.commentTextBox.Size = new System.Drawing.Size(306, 27);
            this.commentTextBox.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(198, 39);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Comment:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(198, 105);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Changed files";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Changesets";
            // 
            // changesListBox
            // 
            this.changesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.changesListBox.FormattingEnabled = true;
            this.changesListBox.Location = new System.Drawing.Point(198, 123);
            this.changesListBox.Name = "changesListBox";
            this.changesListBox.Size = new System.Drawing.Size(306, 238);
            this.changesListBox.TabIndex = 1;
            // 
            // changesetList
            // 
            this.changesetList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.changesetList.FormattingEnabled = true;
            this.changesetList.Location = new System.Drawing.Point(8, 58);
            this.changesetList.Name = "changesetList";
            this.changesetList.Size = new System.Drawing.Size(184, 303);
            this.changesetList.TabIndex = 0;
            this.changesetList.SelectedIndexChanged += new System.EventHandler(this.ShowChangeset);
            // 
            // MiniGUI
            // 
            this.ClientSize = new System.Drawing.Size(523, 414);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MiniGUI";
            this.Text = "Plastic SCM mini GUI";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        private string mSampleRep;
        private Label label1;
        private Label label2;
        private TextBox textBox1;
        private Label label3;
        private ListBox itemsNotAdded;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TextBox commentTextBox;
        private Label label6;
        private Label label5;
        private Label label4;
        private ListBox changesListBox;
        private ListBox changesetList;
        private Button button1;
        private Button button3;
        private Button button2;
        private ListBox itemsToCommit;
    }
}