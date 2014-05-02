using System;
using System.Collections.Generic;

namespace sampleplasticapi
{
    internal class CliArgumentParser
    {
        internal CliArgumentParser(string[] args)
        {
            Parse(args);
        }

        internal bool IsCorrect { get { return mbIsCorrect; } }
        internal string RepositoryName { get { return mRepositoryName; } }
        internal string WorkspaceName { get  { return mWorkspaceName; } }

        internal bool HasOption(string option)
        {
            return mOptions.ContainsKey(option);
        }

        private void Parse(string[] args)
        {
            if (!IsCommandLineCorrect(args))
                return;

            mbIsCorrect = true;

            mRepositoryName = args[0];
            mWorkspaceName = args[1];

            ProcessOptionalArguments(args);
        }

        private bool IsCommandLineCorrect(string[] args)
        {
            if (!HasValidMandatoryArgs(args))
                return false;

            return HasValidOptionalArgs(args);
        }

        private bool HasValidMandatoryArgs(string[] args)
        {
            if (args.Length < NUMBER_OF_MANDATORY_ARGS)
                return false;

            for (int i = 0; i < NUMBER_OF_MANDATORY_ARGS; i++)
                if (args[i].StartsWith("-"))
                    return false;

            return true;
        }

        private bool HasValidOptionalArgs(string[] args)
        {
            for (int i = 2; i < args.Length; i++)
            {
                if (!IsValidOption(args[i]))
                    return false;
            }
            return true;
        }

        private bool IsValidOption(string argument)
        {
            if (IsLongParameterArgument(argument))
            {
                return argument.Substring(2) == CliOptions.LocalArgument;
            }
            return false;
        }

        private bool IsLongParameterArgument(string argument)
        {
            if (argument.Length < 3)
                return false;

            return argument.StartsWith("--");
        }

        private void ProcessOptionalArguments(string[] arguments)
        {
            for (int i = NUMBER_OF_MANDATORY_ARGS; i < arguments.Length; i++)
                if (IsLongParameterArgument(arguments[i]))
                    ProcessLongOption(arguments[i]);
        }

        private void ProcessLongOption(string arg)
        {
            mOptions.Add(arg.Substring(2), true);
        }

        private bool mbIsCorrect = false;

        private string mRepositoryName = string.Empty;
        private string mWorkspaceName = string.Empty;

        private Dictionary<string, object> mOptions = new Dictionary<string, object>();

        private const int NUMBER_OF_MANDATORY_ARGS = 2;
    }

    internal static class CliOptions
    {
        internal const string LocalArgument = "local";
    }
}

