using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Net;
using System.ComponentModel;
using Ionic.Zip;
using System.Text.RegularExpressions;

namespace Metro_Skin_Installer
{
    class InstallActions
    {
        public static System.Uri GetLatestMetro()
        {
            System.Uri LatestURI = new System.Uri("https://google.com/");
            WebClient downloadFile = new WebClient();
            string source = Convert.ToString(downloadFile.DownloadString("http://metroforsteam.com"));
            List<string> downloadEventArgs = new List<string>();
            var regex = Regex.Match(source, "href=\"downloads(\\/*.*.zip)\"");
            LatestURI = new System.Uri("http://metroforsteam.com/downloads" + Convert.ToString(regex.Groups[1].Value));
            return LatestURI;
        }
        public static string FindSteamDir()
        {
            using (var registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam"))
            {
                string filePath = null;
                var regFilePath = registryKey?.GetValue("SteamPath");
                if (regFilePath != null)
                {
                    filePath = Path.Combine(regFilePath.ToString().Replace(@"/", @"\"), "skins");
                }
                return filePath;
            }
        }
        public static void InstallSkin(string steamDir)
        {
            bool customStylesExists;
            if (File.Exists(steamDir + "\\Metro 4.2.4\\custom.styles") && isPatch != "True")
            {
                customStylesExists = true;
                File.Copy(steamDir + "\\Metro 4.2.4\\custom.styles", Path.GetTempPath() + "custom.styles", true);
            }
            using (ZipFile SteamSkin = ZipFile.Read(Path.GetTempPath() + "officialskin.zip"))
            {
                SteamSkin.ExtractAll(FindSteamDir(), ExtractExistingFileAction.OverwriteSilently);
            }
        }
        public static bool CheckSteamSkinDirectoryExists(string SteamDir)
        {
            bool DirExists = false;
            if (Directory.Exists(SteamDir))
            {
                DirExists = true;
            }
            return DirExists;
        }

        public static void TempExtractPatch()
        {
            using (ZipFile patchZip = ZipFile.Read(Path.GetTempPath() + "installer.zip"))
            {
                patchZip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
            }
        }
        public static List<string> DetectExtras()
        {
            string[] manifest = File.ReadAllLines(Path.GetTempPath() + "UPMetroSkin-installer\\manifest.txt");

            List<string> ExtrasList = new List<string> { };
            for (int i = 0; i <= manifest.Length - 1; i++)
            {

                if (Regex.Match((manifest[i].Replace("\\", "")), "\"(.*?)\";\"(.*?)\";\"(.*?)\";\"(.*?)\"").Groups[3].Value == "")
                {
                    string getNameFromManifest = Regex.Match((manifest[i].Replace("\\", "")), "\"(.*?)\";\"(.*?)\";\"(.*?)\";\"(.*?)\"").Groups[1].Value;
                    ExtrasList.Add(getNameFromManifest);
                }
            }
            return ExtrasList;
        }
    }
}
