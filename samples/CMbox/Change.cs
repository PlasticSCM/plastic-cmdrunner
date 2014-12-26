using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CmdRunnerExamples
{
    class Change
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public ChangeType ChangeType { get; set; }

        public Change(string resultRow)
        {
            string[] separated = resultRow.Split(' ');
            ChangeType = GetFromString(separated[0]);
            FullPath = separated[1];
            Name = Path.GetFileName(separated[1]);
        }

        private ChangeType GetFromString(string source)
        {
            switch (source.ToUpperInvariant())
            {
                case "AD+LD":
                case "LD+CO":
                    return CmdRunnerExamples.ChangeType.Deleted;
                case "PR":
                case "AD":
                    return CmdRunnerExamples.ChangeType.Added;
                case "CO":
                    return CmdRunnerExamples.ChangeType.Changed;
                default:
                    return CmdRunnerExamples.ChangeType.None;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, ChangeType);
        }
    }

    enum ChangeType { Added, Deleted, Moved, Changed, None };
}
