using System;
using System.Collections;
using System.Collections.Generic;
using Codice.CmdRunner;

namespace CmdRunnerExamples
{
    public class Replicator
    {
        static void Main(string[] args)
        {
            string server = "localhost:8084";
            string repository = SampleHelper.GenerateRepositoryWithData(server);
            string secondRepository = SampleHelper.GenerateEmptyRepository(server);

            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm find branch on repositories '{0}' --format={{id}}#{{name}} --nototal", repository),
                Environment.CurrentDirectory);

            ArrayList results = SampleHelper.GetListResults(cmdResult, true);
            List<Branch> branches = GetBranchListFromCmdResult(results);

            List<ReplicationResult> resultList = new List<ReplicationResult>();
            foreach (Branch branch in branches)
                resultList.Add(Replicate(branch, repository, secondRepository));

            Console.WriteLine(Environment.NewLine + "Replication complete");
            PrintReplicationResult(resultList);

            Console.ReadLine();

            SampleHelper.RemoveSampleRepository(repository);
            SampleHelper.RemoveSampleRepository(secondRepository);
        }

        private static void PrintReplicationResult(List<ReplicationResult> resultList)
        {
            Console.WriteLine("Branches replicated: {0}" , resultList.Count);
            foreach (ReplicationResult item in resultList)
                Console.WriteLine("- {0} ({1} item{2})",
                    item.Branch.Name, item.Items, item.Items == 1 ? "" : "s");
        }

        private static ReplicationResult Replicate(Branch branch, string repository,
            string secondRepository)
        {
            Console.WriteLine("Replicating branch {0}", branch.Name);

            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
            string.Format("cm replicate \"br:{0}@{1}\" \"rep:{2}\"",
                branch.Name, repository, secondRepository),
            Environment.CurrentDirectory);

            return new ReplicationResult(SampleHelper.GetListResults(cmdResult, true), branch);
        }

        static List<Branch> GetBranchListFromCmdResult(ArrayList results)
        {
            List<Branch> branches = new List<Branch>();
            foreach (string branch in results)
                branches.Add(new Branch(branch));

            return branches;
        }
    }

class ReplicationResult
{
    public long Items { get; set; }
    public Branch Branch { get; set; }

    public ReplicationResult(ArrayList cmdResult, Branch branch)
    {
        Branch = branch;
        string buffer = (string)cmdResult[1];
        Items = long.Parse(buffer.Substring(buffer.LastIndexOf(' ')));
    }
}
}
