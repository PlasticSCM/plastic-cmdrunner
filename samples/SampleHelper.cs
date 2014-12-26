using System;
using System.Collections.Generic;
using System.Text;
using Codice.CmdRunner;
using System.Collections;
using System.IO;
using System.Threading;

namespace CmdRunnerExamples
{
    public class SampleHelper
    {
        public static string GenerateRepositoryWithData(string repServer)
        {
            int numBranches = 5;

            string repositorySpec = GenerateEmptyRepository(repServer);

            GenerateBranches(repositorySpec, numBranches);

            FillBranchesWithRandomItems(numBranches, repositorySpec, false);

            return repositorySpec;
        }

        public static string GenerateEmptyRepository(string repServer)
        {
            string repositorySpec = repositoryName
                + Guid.NewGuid().ToString().Substring(0, 7);

            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm mkrep {0}@{1}", repositorySpec, repServer),
                Environment.CurrentDirectory);

            if (cmdResult.IndexOf("Error") == 0)
                throw new Exception(cmdResult);

            Console.WriteLine("Created repository: {0}", repositorySpec);

            CreateWorkspaceAndSwitchToMain(repositorySpec, repServer);

            return repositorySpec;
        }

        public static string GetWorkspace()
        {
            if (mWorkspace == null)
            {
                mWorkspace = Path.Combine(Path.GetTempPath(),
                    Guid.NewGuid().ToString());

                Directory.CreateDirectory(mWorkspace);
            }

            return mWorkspace;
        }

        public static string AddRandomItem()
        {
            string fileName = "sample" + Guid.NewGuid().ToString().Substring(0, 7);

            using (StreamWriter sw
                = new StreamWriter(Path.Combine(GetWorkspace(), fileName)))
            {
                sw.WriteLine("Testing " + Guid.NewGuid());
            }

            Console.WriteLine("Added {0}", Path.Combine(GetWorkspace(), fileName));
            return fileName;
        }

        public static void CheckinItem(string fileName, string comment)
        {
            string addResult = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm add {0}", fileName),
                GetWorkspace());

            string commitResult = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm ci -c=\"{0}\"", comment),
                GetWorkspace());
        }

        public static void RemoveSampleRepository(string repository)
        {
            Console.WriteLine("Removing sample repository {0}. Please confirm: [y/N]", repository);
            string line = Console.ReadLine();
            if (line != "y")
                return;

            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm rmrep {0}", repository),
                Environment.CurrentDirectory);
        }

        public static ArrayList GetListResults(string cmdresult, bool bLowerCase)
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

        private static void FillBranchesWithRandomItems(int numBranches,
            string repositorySpec,
            bool includeThreadInfo)
        {
            int minRandomItems = 2;
            int maxRandomItems = 6;
            string threadInfo = includeThreadInfo
                ? string.Format("THREAD: {0} ", Thread.CurrentThread.ManagedThreadId)
                : string.Empty;

            Random r = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < numBranches; i++)
            {
                string branch = string.Format("br:/main/task{0}", i);
                CmdRunner.ExecuteCommandWithStringResult(
                    string.Format("cm stb {0}@{1}", branch, repositorySpec),
                    GetWorkspace());

                int items = r.Next(minRandomItems, maxRandomItems);
                Console.WriteLine("{0}Adding {1} random items to {2}",
                    threadInfo, items, branch);

                for (int j = 0; j < items; j++)
                {
                    string filename = AddRandomItem();
                    CheckinItem(filename, "Added " + filename);
                }
            }
        }

        private static void CreateWorkspaceAndSwitchToMain(string repositorySpec,
            string repServer)
        {
            string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm mkwk {0} .", repositorySpec),
                GetWorkspace());

            Console.WriteLine(string.Format("Workspace created at {0}",
                GetWorkspace()));

            cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm stb br:/main@{0}@{1}", repositorySpec,
                repServer), GetWorkspace());
        }

        private static void GenerateBranches(string repServer, int numBranches)
        {
            for (int i = 0; i < numBranches; i++)
            {
                string cmdResult = CmdRunner.ExecuteCommandWithStringResult(
                    string.Format("cm mkbranch br:/main/task{0}", i),
                    GetWorkspace());
            }
        }

        internal static void RandomlyUpdateRepository(string mSampleRep, Random random)
        {
            Thread.Sleep(random.Next(3000, 10000));
            FillBranchesWithRandomItems(random.Next(0, 5), mSampleRep, true);
        }

        internal static void RandomlyUpdateWorkspace(Random random)
        {
            int maxRandomItems = 4;
            int index = random.Next(maxRandomItems);

            for (int j = 0; j < index; j++)
                EditRandomItem(random);

            index = random.Next(maxRandomItems);
            for (int j = 0; j < index; j++)
                AddRandomItem();

            int milisecondsSleep = random.Next(6000, 12000);
            Console.WriteLine("Sleeping {0} seconds", milisecondsSleep / 1000);
            Thread.Sleep(milisecondsSleep);
        }

        private static void EditRandomItem(Random random)
        {
            string[] files = Directory.GetFiles(GetWorkspace()); 
            if (files.Length == 0)
                return;

            int index = random.Next(files.Length);
            string addResult = CmdRunner.ExecuteCommandWithStringResult(
                string.Format("cm co {0}", files[index]),
                GetWorkspace());

            using (StreamWriter sw = new StreamWriter(files[index]))
            {
                sw.WriteLine("Testing " + Guid.NewGuid());
            }

            Console.WriteLine("Edited {0}", files[index]);
        }

        private static string repositoryName = "cmdSample";
        private static string mWorkspace = null;
    }
}
