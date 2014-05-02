using System;
using Codice.CmdRunner;
using System.IO;
using System.Runtime.InteropServices;

namespace sampleplasticapi
{
    class Program
    {
        public static void Main(string[] args)
        {
            CliArgumentParser cli = new CliArgumentParser(args);
            if(!cli.IsCorrect)
            {
                Console.WriteLine(USAGE);
                Environment.ExitCode = WRONG_USAGE_CODE;
                return;
            }

            // The API is implemented to take advantage of the `cm shell' command
            // The first executed command will open a shell which will receive PlasticSCM commands 
            // This optimizes the execution since the CLI environment is initialized only once.
            try
            {
                // Set up our work environment -> repository + workspace
                if (!PlasticAPI.CreateRepository(cli.RepositoryName, out mError))
                    AbortProcess();

                string workspaceDir = Path.Combine(GetBasePath(cli), cli.WorkspaceName);

                if(Directory.Exists(workspaceDir))
                {
                    mError = string.Format("Targeted workspace directory already exists: {0}", workspaceDir);
                    AbortProcess();
                }

                Directory.CreateDirectory(workspaceDir);
                PlasticAPI.WorkingDirectory = workspaceDir;

                if(!PlasticAPI.CreateWorkspace(cli.WorkspaceName, cli.RepositoryName, out mError))
                    AbortProcess();

                // Add some initial contents
                string filePath1 = Path.Combine(workspaceDir, "file 1.txt");
                string filePath2 = Path.Combine(workspaceDir, "file2.txt");

                File.WriteAllText(filePath1, "This is a sample file");
                File.WriteAllText(filePath2, "This is another sample file");

                if(!PlasticAPI.Add(filePath1, false, out mError))
                    AbortProcess();
                if(!PlasticAPI.Add(filePath2, false, out mError))
                    AbortProcess();

                if(!PlasticAPI.Checkin("Initial checkin", out mError))
                    AbortProcess();

                long changesetToLabel = PlasticAPI.GetCurrentChangeset();
                Console.WriteLine("First changeset: {0}", changesetToLabel);

                // Test partial checkin, checkout, undo checkout and move commands
                if(!PlasticAPI.Checkout(new string[] { filePath1 }, out mError))
                    AbortProcess();

                CheckIsItemCheckedOut(filePath1);

                if(!PlasticAPI.UndoCheckout(new string[]{ filePath1 }, out mError))
                    AbortProcess();

                string moveDestination = Path.Combine(workspaceDir, "moved_file.txt");
                if(!PlasticAPI.Move(filePath1, moveDestination, out mError))
                    AbortProcess();
                if(!PlasticAPI.Checkout(new string[] { filePath2 }, out mError))
                    AbortProcess();

                File.AppendAllText(filePath2, "\nThis file has been modified.");

                string directoryPath = Path.Combine(workspaceDir, "src");
                Directory.CreateDirectory(directoryPath);

                string fooPath = Path.Combine(directoryPath, "foo.c");
                File.WriteAllText(fooPath, HELLO_WORLD_C);

                string barPath = Path.Combine(directoryPath, "bar.c");
                File.WriteAllText(barPath, SAMPLE_C_CODE);

                if(!PlasticAPI.Add(directoryPath, true, out mError))
                    AbortProcess();

                CheckIsItemCheckedOut(directoryPath);
                CheckIsItemCheckedOut(fooPath);
                CheckIsItemCheckedOut(barPath);

                if(!PlasticAPI.Checkin(new string[] {
                    filePath2, moveDestination }, "Adding a moved file and a modified one", out mError))
                {
                    AbortProcess();
                }

                CheckIsItemCheckedOut(directoryPath);
                CheckIsItemCheckedOut(fooPath);
                CheckIsItemCheckedOut(barPath);

                if(!PlasticAPI.Checkin(
                    "Check in all remaining contents -> a recursively added directory tree.", out mError))
                {
                    AbortProcess();
                }

                // Update workspace contents and test the 'remove' command
                if(!PlasticAPI.GetLatest(out mError))
                    AbortProcess();

                if(!PlasticAPI.Delete(filePath2, out mError))
                    AbortProcess();

                if(!PlasticAPI.Checkin("Check in the deleted file", out mError))
                    AbortProcess();

                // Create labels and exit
                if(!PlasticAPI.CreateLabel(changesetToLabel, "BL001", "First changeset ever!", out mError))
                    AbortProcess();

                if(!PlasticAPI.CreateLabel(
                    PlasticAPI.GetCurrentChangeset(), "BL002", "Last changeset for today", out mError))
                {
                    AbortProcess();
                }

                Console.WriteLine("Sample run finished OK!");
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("ERROR: {0}", e.Message));
                Environment.ExitCode = GENERIC_ERROR_CODE;
            }
            finally
            {
                CmdRunner.TerminateShell();
            }
        }
        private static void AbortProcess()
        {
            throw new Exception(mError);
        }

        private static string GetBasePath(CliArgumentParser cli)
        {
            if (cli.HasOption(CliOptions.LocalArgument))
                return Environment.CurrentDirectory;
            return Path.GetTempPath();
        }

        private static void CheckIsItemCheckedOut(string path)
        {
            if (PlasticAPI.IsCheckedOut(path))
                return;

            mError = string.Format("File '{0}' should be checked out, but it isn't.", path);
            AbortProcess();
        }

        private static string mError = string.Empty;

        const int GENERIC_ERROR_CODE = 1;
        const int WRONG_USAGE_CODE = 2;

        const string HELLO_WORLD_C =
@"#include <stdio.h>

int main(int argc, char **argv) {
    printf(""Hello, world!\n"");
    return 0;
}
";
        const string SAMPLE_C_CODE =
@"
int my_function(char letter);

float other_function(int number) {
    return my_function(number) * 3.1416;
}

int my_function(char letter) {
    return (int)letter + 10;
}
";

        const string USAGE =
@"Usage:
  sampleplasticapi.exe <repository-name> <workspace-name> [options]

Description:
  This command will execute a set of PlasticSCM basic commands in order to
  illustrate how to build an API taking advantage of how the CLI works.

Requirements:
  The local PlasticSCM CLI client (`cm') must be able to connect to a running
  PlasticSCM server.

Available options:
  --local: Create the sample workspace under the current directory, instead
           of under the temporary directory.

Actions:
  A new repository called <repository-name> will be created on the
  PlasticSCM server that the client is linked to. A new workspace (called
  <workspace-name>) pointing to that repository will be created under
  the temporary directory. It can be created under the current directory
  by using the '--local' parameter.

  Once the PlasticSCM environment has been set up, this program will create
  sample contents that will be uploaded into the repository. Some operations
  will be performed on then, such as renaming, modifying and deleting a file,
  or labelling a changeset.

  We encourage you to review the repository contents after the execution
  using the PlasticSCM graphic or command-line interface.

Examples:
  sampleplasticapi.exe my-repo my-workspace
  sampleplasticapi.exe new-repo wkLocal --local
";
    }
}
