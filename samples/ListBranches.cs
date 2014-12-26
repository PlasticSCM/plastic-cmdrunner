using System;
using System.Collections;
using System.Collections.Generic;
using Codice.CmdRunner;

namespace CmdRunnerExamples
{
    public class ListBranches
    {
        static void Main(string[] args)
        {
            string server = "localhost:8084";
            string repository = SampleHelper.GenerateRepositoryWithData(server);
            Console.WriteLine("Created repository: {0}", repository);

            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm find branch on repositories '{0}' --format={{id}}#{{name}} --nototal", repository),
                Environment.CurrentDirectory);

            ArrayList results = SampleHelper.GetListResults(cmdResult, true);
            List<Branch> branches = GetBranchListFromCmdResult(results);

            foreach (Branch branch in branches)
                Console.WriteLine(branch);

            Console.ReadLine();

            SampleHelper.RemoveSampleRepository(repository);
        }

        static List<Branch> GetBranchListFromCmdResult(ArrayList results)
        {
            List<Branch> branches = new List<Branch>();
            foreach (string branch in results)
                branches.Add(new Branch(branch));

            return branches;
        }
    }

    public class Branch
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Branch(string output)
        {
            string[] parsed = output.Split('#');
            Id = parsed[0];
            Name = parsed[1];
        }

        public override string ToString()
        {
            return Id + " - " + Name;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Branch))
                return base.Equals(obj);

            return ((Branch)obj).Name.Equals(Name);
        }
    }
}
