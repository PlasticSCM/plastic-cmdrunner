using System;
using System.Collections.Generic;
using System.Text;

namespace CmdRunnerExamples
{
    class SavedGame
    {
        public int Colors { get; set; }
        public int Score { get; set; }

        public SavedGame(string line)
        {
            string[] values = line.Split('#');
            Score = int.Parse(values[1]);
            Colors = int.Parse(values[0]);
        }

        public override string ToString()
        {
            return string.Format("{0} colors. Score: {1}", Colors, Score);
        }
    }
}
