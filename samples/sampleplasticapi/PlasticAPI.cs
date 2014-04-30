using System;
using System.IO;

using Codice.CmdRunner;

namespace sampleplasticapi
{
    public static class PlasticAPI
    {
        internal static string WorkingDirectory 
        {
            private get
            {
                if (string.IsNullOrEmpty(mWorkingDirectory))
                    return Environment.CurrentDirectory;
                return mWorkingDirectory;
            }
            set
            {
                mWorkingDirectory = value;
            }
        }

        internal static bool CreateRepository(string name, out string error)
        {
            return ExecuteCommand(string.Format("cm mkrep {0}", name), out error);
        }

        internal static bool CreateWorkspace(string name, string repository, out string error)
        {
            return ExecuteCommand(string.Format(
                "cm mkwk {0} {1} --repository={2}", name, ProtectPath(WorkingDirectory), repository), out error);
        }

        internal static bool GetLatest(out string error)
        {
            return ExecuteCommand(string.Format("cm update --last"), out error);
        }

        internal static bool Checkout(string[] paths, out string error)
        {
            error = string.Empty;

            if (paths.Length == 0)
                return true;

            return ExecuteCommand(string.Format("cm co {0}",PathsToStringParameter(paths)), out error);
        }

        internal static bool Checkin(string comment, out string error)
        {
            return ExecuteCommand(string.Format("cm ci -m \"{0}\"", comment), out error);
        }

        internal static bool Checkin(string[] paths, string comment, out string error)
        {
            error = string.Empty;

            if (paths.Length == 0)
                return true;

            return ExecuteCommand(string.Format(
                "cm ci {0} -m \"{1}\"", PathsToStringParameter(paths), comment), out error);
        }

        internal static bool UndoCheckout(string[] paths, out string error)
        {
            error = string.Empty;

            if (paths.Length == 0)
                return true;

            return ExecuteCommand(string.Format("cm unco {0}", PathsToStringParameter(paths)), out error);
        }

        internal static bool IsCheckedOut(string path)
        {
            string output = ExecuteCommandForOutput(string.Format("cm ls {0} --format={{co}}", ProtectPath(path)));
            string[] splitList = output.Split(new string[] {
                "\r\n", "\r", "\n"
            }, StringSplitOptions.RemoveEmptyEntries);
            if (splitList.Length == 0)
                return false;
            return splitList[0] == "CO";

        }

        internal static bool Add(string path, bool recursive, out string error)
        {
            if (recursive && Directory.Exists(path))
                return ExecuteCommand(string.Format("cm add -R {0}", ProtectPath(path)), out error);
            return ExecuteCommand(string.Format("cm add {0}", ProtectPath(path)), out error);
        }

        internal static bool Move(string source, string destination, out string error)
        {
            return ExecuteCommand(string.Format(
                "cm mv {0} {1}", ProtectPath(source), ProtectPath(destination)), out error);
        }

        internal static bool Delete(string path, out string error)
        {
            return ExecuteCommand(string.Format("cm rm {0}", ProtectPath(path)), out error);
        }

        internal static long GetCurrentChangeset()
        {
            string selector = ExecuteCommandForOutput("cm status -s");
            if (!selector.Contains("@"))
                return -1;
            return ParseChangeset(selector.Split('@')[0]);
        }

        internal static bool CreateLabel(long cset, string name, string comment, out string error)
        {
            return ExecuteCommand(string.Format(
                "cm mklabel \"{0}\" --changeset={1} -m \"{2}\"", name, cset, comment), out error);
        }

        private static string PathsToStringParameter(string[] paths)
        {
            return string.Join(" ", ProtectPaths(paths));
        }

        private static string[] ProtectPaths(string[] unprotectedPaths)
        {
            string[] protectedPaths = new string[unprotectedPaths.Length];

            for (int i = 0; i < protectedPaths.Length; i++)
                protectedPaths[i] = ProtectPath(unprotectedPaths[i]);

            return protectedPaths;
        }

        private static string ProtectPath(string path)
        {
            if (path.StartsWith("\"") && path.EndsWith("\""))
                return path;
            return string.Format("\"{0}\"", path);
        }

        private static long ParseChangeset(string changeset)
        {
            if (!changeset.StartsWith("cs:"))
                return -1;

            long changesetId;
            if (!long.TryParse(changeset.Substring(3), out changesetId))
                return -1;

            return changesetId;
        }

        private static bool ExecuteCommand(string command, out string error)
        {
            string output;
            int result = CmdRunner.ExecuteCommandWithResult(command, WorkingDirectory, out output, out error, true);
            if (string.IsNullOrEmpty(error))
                error = output;
            return result == 0;
        }

        private static string ExecuteCommandForOutput(string command)
        {
            return CmdRunner.ExecuteCommandWithStringResult(command, WorkingDirectory);
        }

        private static string mWorkingDirectory = string.Empty;
    }
}

