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
    public class Game : Form
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Game());
        }

        private ListBox listBox1;
        private Label label2;
        private TextBox textBox1;
        private Button button2;
        private Button button10;
        private Label label3;
        private Button button1;

        private Color[] colors = new Color[] {
            Color.Red,
            Color.Orange,
            Color.Yellow,
            Color.Green,
            Color.Blue,
            Color.Violet
        };

        public Game()
        {
            InitializeComponent();
            button3.BackColor = Color.Red;
            button4.BackColor = Color.Orange;
            button5.BackColor = Color.Yellow;
            button6.BackColor = Color.Green;
            button7.BackColor = Color.Blue;
            button8.BackColor = Color.Violet;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(DisplayColors));
            thread.Start();
        }

        void DisplayColors()
        {
            int previous = -1;
            Color baseColor = button9.BackColor;
            Random random = new Random(DateTime.Now.Millisecond);
            if (mOriginal != null)
            {
                mScore -= 1;
                label1.Invoke((MethodInvoker)delegate
                {
                    label1.Text = string.Format("Score: {0}", mScore);
                });
            }

            mOriginal = new List<Color>();

            for (int i = 0; i < mColors; i++)
            {
                int index = random.Next(colors.Length);
                if (index == previous)
                    index = (index + 1) % colors.Length;

                button9.Invoke((MethodInvoker)delegate
                {
                    button9.Text = string.Empty;
                    button9.BackColor = colors[index]; // runs on UI thread
                    mOriginal.Add(colors[index]);
                });

                previous = index;
                Thread.Sleep(mTime);
            }
            button9.Invoke((MethodInvoker)delegate
            {
                button9.BackColor = baseColor;
                button9.Text = "Now it's your turn";
            });
        }

        private void SaveGame()
        {
            string item = SampleHelper.AddRandomItem();
            SampleHelper.CheckinItem(item, string.Format("{0}#{1}",
                    mColors, mScore));
        }

        private void ReloadSavedGames()
        {
            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm find changeset on repositories '{0}' --format={{comment}} --nototal",
                textBox1.Text),
                Environment.CurrentDirectory);

            ArrayList results = SampleHelper.GetListResults(cmdResult, true);

            listBox1.Items.Clear();
            results.RemoveAt(0);
            foreach (string item in results)
                listBox1.Items.Add(new SavedGame(item));

        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text
                = SampleHelper.GenerateEmptyRepository(Configuration.ServerName);
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            ReloadSavedGames();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SavedGame game = (SavedGame)listBox1.SelectedItem;
            mScore = game.Score;
            mColors = game.Colors;

            mTime = 1000 - (mColors - 3) * 100;

            label1.Text = string.Format("Score: {0}", mScore);
            mOriginal = null;
            mSecond = null;
            button9.Text = "Saved game loaded. Press play";
            this.Invalidate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (mOriginal == null)
            {
                MessageBox.Show("Press play!");
                return;
            }

            if (mSecond == null)
                mSecond = new List<Color>();

            mSecond.Add(button.BackColor);

            if (mSecond.Count == mOriginal.Count)
            {
                for (int i = 0; i < mOriginal.Count; i++)
                {
                    if (mSecond[i] != mOriginal[i])
                    {
                        button9.Text = "Wrong, try again";
                        mSecond = null;
                        return;
                    }
                }

                button9.Text = "Correct! Keep playing";
                mScore += mOriginal.Count;
                mColors += 1;
                mTime -= 100;
                mOriginal = null;
                mSecond = null;

                label1.Text = string.Format("Score: {0}", mScore);
                SaveGame();
                ReloadSavedGames();
            }
        }
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
        private Button button8;
        private Button button9;
        private Label label1;

        private void InitializeComponent()
        {
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(229, 116);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(59, 51);
            this.button3.TabIndex = 4;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(294, 116);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(59, 51);
            this.button4.TabIndex = 5;
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button3_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(359, 116);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(59, 51);
            this.button5.TabIndex = 6;
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button3_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(229, 173);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(59, 51);
            this.button6.TabIndex = 7;
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button3_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(294, 173);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(59, 51);
            this.button7.TabIndex = 8;
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button3_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(359, 173);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(59, 51);
            this.button8.TabIndex = 9;
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button3_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(229, 12);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(189, 51);
            this.button9.TabIndex = 10;
            this.button9.Text = "Play!";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button10_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(380, 254);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Score:";
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 32);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(197, 134);
            this.listBox1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Saved games";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 221);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(197, 20);
            this.textBox1.TabIndex = 14;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(134, 247);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 16;
            this.button2.Text = "Create new";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(53, 247);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(75, 23);
            this.button10.TabIndex = 17;
            this.button10.Text = "Load";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click_1);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 199);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Repository";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(134, 172);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 19;
            this.button1.Text = "Load";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Game
            // 
            this.ClientSize = new System.Drawing.Size(428, 276);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.listBox1);
            this.Name = "Game";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private List<Color> mOriginal;
        private List<Color> mSecond;
        private int mScore = 0;
        private int mColors = 3;
        private int mTime = 1000;
    }
}