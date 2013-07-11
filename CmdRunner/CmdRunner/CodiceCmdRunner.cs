using System;
using System.IO;
using System.Threading;
using System.Diagnostics;


namespace Codice.CmdRunner
{
    internal class CodiceCmdRunner : BaseCmdRunner
    {
        protected int ReadFileOutputUnix(
            Process proc,
            string outputfile,
            out string output,
            out string error)
        {
            bool bDone = false;
            int result = 1;

            output = string.Empty;
            error = string.Empty;

            StreamReader sreader = null;
            try
            {
                while (!bDone)
                {
                    int initWait = Environment.TickCount;
                    double totSeconds = GetTotalProcessorTime(proc);
                    int lastElapsed = Environment.TickCount;

                    while (!ReadFileContent(outputfile, ref output) && !proc.HasExited)
                    {
                        result = RunWait(
                            proc, initWait,
                            ref lastElapsed,
                            ref totSeconds,
                            out error);

                        if (result != 0)
                            return result;
                    }

                    if (output.IndexOf(COMMAND_RESULT) < 0)
                        return 1;

                    bDone = true;

                    string commandResultStr = output.Substring(
                        output.IndexOf(COMMAND_RESULT) + COMMAND_RESULT.Length + 1);

                    result = Convert.ToInt32(commandResultStr);
                    output = output.Substring(0, output.IndexOf(COMMAND_RESULT));
                }
            }
            finally
            {
                if (sreader != null)
                    sreader.Close();
                if (File.Exists(outputfile))
                    File.Delete(outputfile);
            }

            return result;
        }

        protected int ReadFileOutputWindows(Process proc, string outputfile, out string output,
            out string error)
        {
            bool bDone = false;
            int result = 1;

            output = string.Empty;
            error = string.Empty;

            while (!bDone)
            {
                int initWait = Environment.TickCount;
                double totSeconds = GetTotalProcessorTime(proc);
                int lastElapsed = Environment.TickCount;

                StreamReader sreader = null;

                while (!TryToOpenFile(outputfile, ref sreader) && !proc.HasExited)
                {
                    result = RunWait(
                        proc, initWait,
                        ref lastElapsed,
                        ref totSeconds,
                        out error);

                    if (result != 0)
                        return result;
                }

                try
                {
                    if (sreader != null)
                    {
                        while (sreader.Peek() >= 0 & !bDone) // we skip results after the last line
                        {
                            string line = sreader.ReadLine();
                            if (line.StartsWith(COMMAND_RESULT))
                            {
                                bDone = true;
                                result = Convert.ToInt32(line.Substring(COMMAND_RESULT.Length + 1));
                            }
                            else
                                output += line + "\n";
                        }
                    }
                }
                finally
                {
                    if (sreader != null)
                        sreader.Close();

                    if (File.Exists(outputfile))
                        File.Delete(outputfile);
                }
            }

            return result;
        }

        protected int ReadStdOutput(Process proc, out string output, out string error)
        {
            ReadAsync reader = new ReadAsync(ReadALine);
            bool bDone = false;

            output = string.Empty;
            error = string.Empty;

            int result = 1;

            while (!bDone)
            {
                IAsyncResult aresult = reader.BeginInvoke(proc.StandardOutput, null, null);

                int initWait = Environment.TickCount;
                double totSeconds = GetTotalProcessorTime(proc);
                int lastElapsed = Environment.TickCount;

                while (!aresult.IsCompleted)
                {
                    result = RunWait(
                        proc, initWait,
                        ref lastElapsed,
                        ref totSeconds,
                        out error);
                    if (result != 0)
                        return result;
                }

                string line = reader.EndInvoke(aresult);

                if (line.StartsWith(COMMAND_RESULT))
                {
                    bDone = true;
                    result = Convert.ToInt32(line.Substring(COMMAND_RESULT.Length + 1));
                }
                else
                    output += line + "\n";
            }
            return result;
        }

        protected int RunWait(Process proc, int initWait, ref int lastElapsed,
            ref double totSeconds, out string error)
        {
            Thread.Sleep(10);
            error = string.Empty;

            if (!mbTimeOut)
            {
                return 0;
            }

            double procTime = GetTotalProcessorTime(proc);

            long elapsed = Environment.TickCount - initWait;

            if (Environment.TickCount - lastElapsed > GetSpinTime())
            {
                WriteLine(
                    string.Format(
                        "RunAndWait spinning. Time {0}. Proc time {1}",
                        Environment.TickCount - initWait, procTime));
                lastElapsed = Environment.TickCount;

                // if there was no movement in the process
                if (procTime == totSeconds)
                {
                    // try to move the input something
                    WriteLine("Sending an enter to move things a little bit");
                    proc.StandardInput.WriteLine(string.Empty);
                }
            }

            if (elapsed > GetMaxWaitTime())
            {
                if (totSeconds == -1)
                {
                    if (elapsed > GetMaxWaitTimeAll())
                    {
                        WriteLine("Too much time waiting for comand result");
                        error = "Too much time waiting to command result";
                        return 1;
                    }
                    // Don't waste all the time in this check when we reach to a minute waiting
                    System.Threading.Thread.Sleep(10);
                }
                else if (procTime == totSeconds)
                {
                    // tired of waiting...
                    WriteLine("Too much time waiting to read");
                    error = "Too much time waiting to read... FED UP!";
                    return 1;
                }
                totSeconds = GetTotalProcessorTime(proc);
            }

            return 0;
        }

        protected string ReadALine(StreamReader reader)
        {
            return reader.ReadLine();
        }

        internal void DontTimeout()
        {
            mbTimeOut = false;
        }

        internal void WorkWithoutFileCommunication()
        {
            USE_FILE_COMMUNICATION = false;
        }

        internal override int RunAndWait(string cmd, string workingdir, out string output,
            out string error)
        {
            return RunAndWait(cmd, workingdir, out output, out error, true);
        }

        internal override int RunAndWait(string cmd, string workingdir, out string output,
            out string error, bool bShell)
        {
            if (!bShell || !cmd.StartsWith("cm"))
            {
                return base.RunAndWait(cmd, workingdir, out output, out error);
            }

            workingdir = Path.GetFullPath(workingdir);
            if (mCmdProc == null)
            {
                mCmdProc = InitCmdProc(workingdir);
            }

            string command = cmd.Substring(3);
            string outputfile = string.Empty;

            if (USE_FILE_COMMUNICATION)
            {
                outputfile = Path.GetTempFileName();

                if (File.Exists(outputfile))
                    File.Delete(outputfile);

                string cmdText = string.Format(
                    "{0} -path=\"{1}\" --shelloutputfile=\"{2}\" --stack",
                    command, workingdir, outputfile);

                mCmdProc.StandardInput.WriteLine(cmdText);
            }
            else
            {
                string cmdText = string.Format("{0} -path=\"{1}\"",
                    command, workingdir);

                mCmdProc.StandardInput.WriteLine(cmdText);
            }

            output = string.Empty;
            int result = 0;

            if (USE_FILE_COMMUNICATION)
                if (PlatformIdentifier.IsWindows())
                    result = ReadFileOutputWindows(mCmdProc, outputfile,
                        out output, out error);
                else
                    result = ReadFileOutputUnix(mCmdProc, outputfile,
                        out output, out error);
            else
                result = ReadStdOutput(mCmdProc, out output, out error);

            return result;
        }

        private int GetSpinTime()
        {
            if (mSpinTime != -1) return mSpinTime;

            mSpinTime = DEFAULT_SPIN_TIME;

            return mSpinTime;
        }

        private int GetMaxWaitTime()
        {
            if (mMaxWaitTime != -1) return mMaxWaitTime;

            mMaxWaitTime = DEFAULT_MAX_WAIT_TIME;

            return mMaxWaitTime;
        }

        private int GetMaxWaitTimeAll()
        {
            if (mMaxWaitTimeAll != -1) return mMaxWaitTimeAll;

            mMaxWaitTimeAll = DEFAULT_MAX_WAIT_TIME_ALL;

            return mMaxWaitTimeAll;
        }

        private bool TryToOpenFile(string filename, ref StreamReader streamReader)
        {
            if (!File.Exists(filename))
                return false;

            try
            {
                streamReader = new StreamReader(filename);
                streamReader.Peek();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ReadFileContent(string filename, ref string output)
        {
            if (!File.Exists(filename))
                return false;

            StreamReader streamReader = null;
            try
            {
                streamReader = new StreamReader(filename);
                output = streamReader.ReadToEnd();
                if (output.IndexOf(COMMAND_RESULT) >= 0)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (streamReader != null)
                    streamReader.Close();
            }
        }

        private Process InitCmdProc(string workingdir)
        {
            string shellcmd = LaunchCommand.Get().GetCmShellCommand();
            shellcmd = shellcmd.Replace("[GENDATESTAMP]", DateTime.Now.ToString(
                "yyyy-MM-dd-hh-mm-ss"));

            string cpath = LaunchCommand.Get().GetClientPath();
            if (cpath != string.Empty)
            {
                cpath = Path.GetFullPath(cpath);
                shellcmd = shellcmd.Replace("[CLIENTPATH]", cpath);
            }

            Process result = InternalRun(shellcmd, workingdir, true);

            string line;
            do
            {
                line = result.StandardOutput.ReadLine();
                WriteLine(line);

            } while (line.IndexOf("Plastic SCM shell") < 0);

            return result;
        }

        protected bool USE_FILE_COMMUNICATION = true; // communicate with cm shell using a file

        private const int DEFAULT_SPIN_TIME = 10 * 1000;
        private const int DEFAULT_MAX_WAIT_TIME = 8 * 60 * 1000;
        private const int DEFAULT_MAX_WAIT_TIME_ALL = 30 * 60 * 1000;

        private int mSpinTime = -1;
        private int mMaxWaitTime = -1;
        private int mMaxWaitTimeAll = -1;

        private bool mbTimeOut = true;

        private static string COMMAND_RESULT = "CommandResult";

        delegate string ReadAsync(StreamReader reader);
    }
}
