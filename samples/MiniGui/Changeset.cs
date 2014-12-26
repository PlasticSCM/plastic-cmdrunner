using System;
using System.Collections.Generic;
using System.Text;

namespace CmdRunnerExamples
{
    public class Changeset
    {
        public string Date { get; set; }
        public string Id { get; set; }
        public string Comment { get; set; }
        public List<Item> Changes { get; set; }

        public Changeset(string output)
        {
            string[] parsed = output.Split('#');
            Id = parsed[0];
            Date = parsed[1];
            Comment = parsed[2];
            Changes = new List<Item>();
        }

        public override string ToString()
        {
            return string.Format("{0}: cs:{1}", Date, Id);
        }
    }
}
