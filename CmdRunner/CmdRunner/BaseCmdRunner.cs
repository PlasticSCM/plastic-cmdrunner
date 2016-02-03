using System;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace Codice.CmdRunner
{
    public interface IConsoleWriter
    {
        void WriteLine(string s);
    }

    internal class BaseCmdRunner
    {
        public void InitConsole(IConsoleWriter writer)
        {
            mConsoleWriter = writer;
        }

        public int InternalExecuteCommand(string command, string path)
        {
            WriteCommand(path, command);

            try
            {
                int result = RunAndWait(command, path);

                ProcessCommandResult(command, result);

                return result;
            }
            catch (Exception e)
            {
                return ManageException(command, path, e);
            }
        }

        public int InternalExecuteCommand(
            string command, string path, string input, out string output, out string error,
            bool bUseCmShell)
        {
            output = string.Empty;
            error = string.Empty;

            WriteCommand(path, command);
            try
            {
                int result;

                if (input != null)
                {
                    result = RunAndWaitWithInput(command, path, input, out output, out error);
                }
                else
                {
                    result = RunAndWait(command, path, out output, out error, bUseCmShell);
                }

                WriteLine(output);

                if (result != 0)
                {
                    WriteLine(error);
                }

                ProcessCommandResult(command, result);

                return result;
            }
            catch (Exception e)
            {
                return ManageException(command, path, e);
            }
        }

        protected void WriteLine(string s)
        {
            if (mConsoleWriter == null)
                return;

            mConsoleWriter.WriteLine(s);
        }

        protected double GetTotalProcessorTime(Process proc)
        {
            try
            {
                return proc.TotalProcessorTime.TotalSeconds;
            }
            catch
            {
                return -1;
            }
        }

        internal Process InternalRun(string cmd, string workingdir, bool bRedirectStreams)
        {
            Process p = new Process();
            string[] args = cmd.Split(' ');
            p.StartInfo.FileName = args[0];
            p.StartInfo.WorkingDirectory = workingdir;
            p.StartInfo.Arguments = EscapeArgs(cmd.Substring(args[0].Length));
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = bRedirectStreams;
            p.StartInfo.RedirectStandardInput = bRedirectStreams;
            p.StartInfo.RedirectStandardError = bRedirectStreams;
            p.StartInfo.UseShellExecute = false;

            if (mEnvironmentVariables != null)
            {
                foreach (string key in mEnvironmentVariables.Keys)
                {
                    p.StartInfo.EnvironmentVariables[key] =
                        mEnvironmentVariables[key] as string;
                }
            }

            p.Start();
            return p;
        }

        internal void SetEnvironmentVariables(Hashtable variables)
        {
            mEnvironmentVariables = variables;
        }

        internal virtual int RunAndWait(string cmd, string workingdir, out string output,
            out string error, bool bUseCmShell)
        {
            return RunAndWaitWithInput(cmd, workingdir, null, out output, out error);
        }

        internal virtual int RunAndWait(string cmd, string workingdir, out string output,
            out string error)
        {
            return RunAndWaitWithInput(cmd, workingdir, null, out output, out error);
        }

        internal virtual int RunAndWaitWithInput(string cmd, string workingdir, string input,
            out string output, out string error)
        {
            Process p = InternalRun(cmd, workingdir, true);
            try
            {
                if (input != null && input != string.Empty)
                {
                    if (!PlatformIdentifier.IsWindows())
                    {
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(input);
                        p.StandardInput.BaseStream.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        p.StandardInput.Write(input);
                    }
                    p.StandardInput.Flush();
                    p.StandardInput.Close();
                }

                output = p.StandardOutput.ReadToEnd();
                error = p.StandardError.ReadToEnd();
                p.WaitForExit();

                return p.ExitCode;
            }
            finally
            {
                p.Close();
            }
        }

        internal virtual int RunAndWait(string cmd, string workingdir)
        {
            Process p = InternalRun(cmd, workingdir, false);
            try
            {
                p.WaitForExit();
                return p.ExitCode;
            }
            finally
            {
                p.Close();
            }
        }

        private bool IsWindows()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    return true;
                default:
                    return false;
            }
        }

        private string EscapeArgs(string args)
        {
            if (IsWindows())
                return args;
            else
                return args.Replace("#", "\\#");
        }

        private int ManageException(string command, string path, Exception e)
        {
            string errormsg = string.Format("Error executing command {0} on path {1}. Error = {2}",
                command, path, e.Message + e.StackTrace);
            WriteLine(errormsg);

            return 1;
        }

        private void WriteCommand(string path, string command)
        {
            string cmdPrint = string.Format("{0}$ {1}", path, command);

            WriteLine(cmdPrint);
        }

        private void ProcessCommandResult(string command, int result)
        {
            if (result == 0)
                return;

            WriteLine(string.Format("Command {0} failed with error code {1}", command, result));
        }

        private Hashtable mEnvironmentVariables = null;
        private static IConsoleWriter mConsoleWriter = null;
        internal Process mCmdProc = null;
    }
}