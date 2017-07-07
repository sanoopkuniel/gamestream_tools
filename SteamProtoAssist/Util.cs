using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SteamProtoAssist
{
    class Util
    {
        public static void RegisterProtocol(string proto, string appPath)
        {
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(proto); 

            key = Registry.ClassesRoot.CreateSubKey(proto);

            key.SetValue(string.Empty, "URL: " + proto + " Protocol");
            key.SetValue("URL Protocol", string.Empty);

            key = key.CreateSubKey(@"shell\open\command");
            key.Close();

            Registry.SetValue(@"HKEY_CLASSES_ROOT\steam\shell\open\command", "","\"" + appPath + "\" \"%1\"");
        }

        public static void ResetProtocol()
        {
            string steamExe = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamExe", null);
            if (steamExe != null)
            {
                steamExe = steamExe.Replace("/", "\\");
                RegisterProtocol("steam", steamExe);
            }
        }

    }
}
