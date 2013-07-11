using System;
using System.Collections;
using System.Text;

using Codice.CmdRunner;

namespace CmdRunnerExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            PrintCmVersion();
            PrintCmChangesets();
            PrintCmBranches();
            PrintCmLabels();
            PrintCmRevisionsOnMainBranch();
            MkWkCommand();
            UpdateCommand();
            SomeCommandsInShell();

            Console.ReadLine();
        }

        private static void PrintCmRevisionsOnMainBranch()
        {
            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                "cm find revision where branch='main' " +
                " --format=\"{item}#{branch}#{changeset}\" on repository 'default' --nototal",
                Environment.CurrentDirectory);

            Console.WriteLine("Revisions on main branch:");
            PrintResult(cmdResult);
            Console.WriteLine();
        }

        private static void PrintCmChangesets()
        {
            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                "cm find changesets on repository 'default' --nototal",
                Environment.CurrentDirectory);

            Console.WriteLine("Changesets on default repository:");
            PrintResult(cmdResult);
            Console.WriteLine();
        }

        private static void PrintCmLabels()
        {
            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                "cm find labels on repository 'default' --nototal",
                Environment.CurrentDirectory);

            Console.WriteLine("Labels on default repository:");
            PrintResult(cmdResult);
            Console.WriteLine();
        }

        private static void PrintCmBranches()
        {
            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                "cm find branches on repository 'default' --nototal",
                Environment.CurrentDirectory);

            Console.WriteLine("Branches on default repository:");
            PrintResult(cmdResult);
            Console.WriteLine();
        }

        private static void PrintCmVersion()
        {
            string cmVersion =
                CmdRunner.ExecuteCommandWithStringResult(
                    "cm version",
                    Environment.CurrentDirectory);

            Console.WriteLine(string.Format("The cm version is: {0}", cmVersion));
            Console.WriteLine();
        }

        private static void MkWkCommand()
        {
            int result = CmdRunner.ExecuteCommandWithResult(
                String.Format("cm mkwk test {0}", Environment.CurrentDirectory),
                Environment.CurrentDirectory);

            Console.WriteLine(string.Format("The mkwk command executed with result: " + result));
        }

        private static void UpdateCommand()
        {
            int result = CmdRunner.ExecuteCommandWithResult(
                String.Format("cm upd {0}", Environment.CurrentDirectory),
                Environment.CurrentDirectory);
            Console.WriteLine(string.Format("The update command finished with result: " + result));
        }

        private static void SomeCommandsInShell()
        {
            string repos = CmdRunner.ExecuteCommandWithStringResult(
                "cm lrep",
                Environment.CurrentDirectory, true);

            /* TODO: more commands here!! */

            string output;
            string error;
            CmdRunner.ExecuteCommandWithResult(
                "cm rmwk .",
                Environment.CurrentDirectory, out output, out error, true);

            Console.WriteLine("The Plastic server has the following repositories: ");
            PrintResult(repos);
            Console.WriteLine(String.Format("The workspace was deleted. Output: {0}. Error: {1} ",
                output, error));
        }

        private static void PrintResult(string cmdResult)
        {
            ArrayList listResult = GetListResults(cmdResult, false);

            foreach (string result in listResult)
            {
                Console.WriteLine(result);
            }
        }

        private static ArrayList GetListResults(string cmdresult, bool bLowerCase)
        {
            if (cmdresult == string.Empty)
                return new ArrayList();

            //conversion
            if (cmdresult.Length >= 2)
            {
                string sub = cmdresult.Substring(cmdresult.Length - 2);
                if (sub.Equals("\r\n"))
                {
                    cmdresult = cmdresult.Substring(0, cmdresult.Length - 2);
                }
                else
                {
                    sub = cmdresult.Substring(cmdresult.Length - 1);
                    if (sub.Equals("\n"))
                        cmdresult = cmdresult.Substring(0, cmdresult.Length - 1);
                }
            }
            cmdresult = cmdresult.Replace("\r\n", "\n");
            cmdresult = cmdresult.Replace("\r", "\n");

            ArrayList al = new ArrayList(cmdresult.Split('\n'));

            if (bLowerCase)
            {
                for (int i = 0; i < al.Count; i++)
                {
                    al[i] = ((string)al[i]).ToLower();
                }
            }
            return al;
        }
    }
}
