using System;
using System.Collections.Generic;
using System.Text;
using Codice.CmdRunner;

namespace CmdRunnerExamples
{
    class HelloWorld
    {
        static void Main(string[] args)
        {
            string server = "localhost:8084";
            string sampleRep = SampleHelper.GenerateRepositoryWithData(server);

            string cmVersion = CmdRunner.ExecuteCommandWithStringResult("cm version",
                Environment.CurrentDirectory);

            Console.WriteLine(string.Format("The cm version is: {0}", cmVersion));

            string repoList = CmdRunner.ExecuteCommandWithStringResult(string.Format("cm lrep {0}", server),
                Environment.CurrentDirectory);

            Console.WriteLine(string.Format("{0}", repoList));

            Console.ReadLine();

            SampleHelper.RemoveSampleRepository(server);
        }
    }
}
