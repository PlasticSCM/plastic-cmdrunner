using System;
using System.Collections.Generic;
using System.Text;

namespace CmdRunnerExamples
{
    public class Item
    {
        public string Path { get; set; }
        public string Status { get; set; }

        public Item(string output)
        {
            if (string.IsNullOrEmpty(output))
            {
                Path = string.Empty;
                Status = string.Empty;
                return;
            }

            string[] parsed = output.Split('#');
            Path = parsed[0].Replace(SampleHelper.GetWorkspace().ToLowerInvariant(),
                "");
            Status = parsed[1];
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Status))
                return string.Empty;

            return string.Format("{0} ({1})", Path, Status);
        }
    }
}
