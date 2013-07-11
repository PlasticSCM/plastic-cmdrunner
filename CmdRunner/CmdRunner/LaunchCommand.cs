using System;
using System.IO;
using System.Xml.Serialization;

namespace Codice.CmdRunner
{
    [Serializable]
    public class LaunchCommandConfig
    {
        public string FullServerCommand;
        public string CmShellComand;
        public string AllServerPrefixCommand;
        public string ClientPath;
    }

    public class LaunchCommand
    {
        private static LaunchCommandConfig LoadFromFile(string file)
        {
            FileStream reader = new FileStream(file, FileMode.Open, FileAccess.Read);
            XmlSerializer ser = new XmlSerializer(typeof(LaunchCommandConfig));
            return (LaunchCommandConfig)ser.Deserialize(reader);
        }

        private LaunchCommandConfig mConfig;
        private LaunchCommand()
        {
            string file = "launchcommand.conf";
            if (File.Exists(file))
            {
                mConfig = LoadFromFile(file);
            }
            else
            {
                mConfig = new LaunchCommandConfig();
            }

            if (mConfig.FullServerCommand == null || mConfig.FullServerCommand == string.Empty)
                mConfig.FullServerCommand = "[SERVERPATH]plasticd --console";

            if (mConfig.CmShellComand == null || mConfig.CmShellComand == string.Empty)
                mConfig.CmShellComand = string.Format("{0} shell --logo", mExecutablePath);

            if (mConfig.ClientPath == null)
                mConfig.ClientPath = string.Empty;

            if (mConfig.CmShellComand == null)
                mConfig.CmShellComand = string.Empty;
        }

        private static LaunchCommand mInstance = null;
        private static string mExecutablePath = "cm"; // executable cm

        public static void SetExecutablePath(string executablepath)
        {
            mExecutablePath = executablepath;
        }

        public static LaunchCommand Get()
        {
            if (mInstance == null)
                mInstance = new LaunchCommand();

            return mInstance;
        }

        public string GetFullServerCommand()
        {
            return mConfig.FullServerCommand;
        }

        public string GetCmShellCommand()
        {
            return mConfig.CmShellComand;
        }

        public string GetAllServerPrefixCommand()
        {
            return mConfig.AllServerPrefixCommand;
        }

        public string GetClientPath()
        {
            return mConfig.ClientPath;
        }

    }

}
