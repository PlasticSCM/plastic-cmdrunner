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
            // The API is implemented to take advantage of the `cm shell' command
            // The first executed command will open a shell which will receive PlasticSCM commands 
            // This optimizes the execution since the CLI environment is initialized only once.
            try
            {
                // Set up our work envirnoment -> repository + workspace
                string workspaceDir = Path.Combine(Path.GetTempPath(), WORKSPACE_NAME);
                if (!PlasticAPI.CreateRepository(REPOSITORY_NAME, out mError))
                    AbortProcess();
                
                Directory.CreateDirectory(workspaceDir);
                PlasticAPI.WorkingDirectory = workspaceDir;

                if(!PlasticAPI.CreateWorkspace(WORKSPACE_NAME, REPOSITORY_NAME, out mError))
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

        private static void CheckIsItemCheckedOut(string path)
        {
            if (PlasticAPI.IsCheckedOut(path))
                return;

            mError = string.Format("File '{0}' should be checked out, but it isn't.", path);
            AbortProcess();
        }

        private static string mError = string.Empty;

        const string REPOSITORY_NAME = "my_repo";
        const string WORKSPACE_NAME = "my_wk";

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


    }
}
