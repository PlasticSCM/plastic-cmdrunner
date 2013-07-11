using System;

namespace Codice.CmdRunner
{
    public class PlatformIdentifier
    {
        public static bool IsWindows()
        {
            if (!bIsWindowsInitialized)
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        bIsWindows = true;
                        break;
                }
                bIsWindowsInitialized = true;
            }
            return bIsWindows;
        }

        public static bool IsMac()
        {
            if (!bIsMacInitialized)
            {
                if (!IsWindows())
                {
                    // The first versions of the framework (1.0 and 1.1)
                    // didn't include any PlatformID value for Unix,
                    // so Mono used the value 128. The newer framework 2.0
                    // added Unix to the PlatformID enum but,
                    // sadly, with a different value: 4 and newer versions of
                    // .NET distinguished between Unix and MacOS X,
                    // introducing yet another value 6 for MacOS X.

                    System.Version v = Environment.Version;
                    int p = (int)Environment.OSVersion.Platform;

                    if ((v.Major >= 3 && v.Minor >= 5) ||
                        (IsRunningUnderMono() && v.Major >= 2 && v.Minor >= 2))
                    {
                        //MacOs X exist in the enumeration
                        bIsMac = p == 6;
                    }
                    else
                    {
                        if ((p == 4) || (p == 128))
                        {
                            int major = Environment.OSVersion.Version.Major;

                            // Darwin tiger is 8, darwin leopard is 9,
                            // darwin snow leopard is 10
                            // This is not very nice, as it may conflict
                            // on other OS like Solaris or AIX.
                            bIsMac = (major == 8 || major == 9 || major == 10);
                        }
                    }
                }

                bIsMacInitialized = true;
            }

            return bIsMac;
        }

        private static bool IsRunningUnderMono()
        {
            Type t = Type.GetType("Mono.Runtime");

            return (t != null);
        }

        private static bool bIsWindowsInitialized = false;
        private static bool bIsWindows = false;

        private static bool bIsMacInitialized = false;
        private static bool bIsMac = false;
    }
}
