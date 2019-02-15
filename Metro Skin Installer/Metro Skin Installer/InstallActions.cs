using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Ionic.Zip;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.Web.Script.Serialization;
using System.Diagnostics;

namespace Metro_Skin_Installer
{
    class InstallActions
    {
        public const string SkinFolder = "\\"+"MetroSkin";
        public static void UpdateCheck()
        {
            WebClient GitHubAPI = new WebClient();
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            string jsonResponse = null;
            GitHubAPI.Headers.Add("user-agent", "MetroSkinInstaller");
            jsonResponse = GitHubAPI.DownloadString(@"https://api.github.com/repos/henrikx/metroskininstaller/releases");
            Dictionary<string,dynamic>[] ReleaseData = jsonParser.Deserialize<Dictionary<string,dynamic>[]>(jsonResponse);
            if ((ReleaseData[0])["tag_name"] != "v" + Application.ProductVersion)
            {
                if (MessageBox.Show(null, "An update is available! Download now?", "Update", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Process.Start(@"https://github.com/henrikx/metroskininstaller/releases");
                }
            }
        }
        public static System.Uri GetLatestMetro()
        {
            System.Uri LatestURI = new System.Uri("https://google.com/");
            WebClient downloadFile = new WebClient();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            string source = "";
            try
            {
                source = Convert.ToString(downloadFile.DownloadString("http://metroforsteam.com"));
                err_ARCHIVE = false;
            }
            catch (System.Net.WebException e)
            {
                MessageBox.Show(e.Message); // No internet
                err_ARCHIVE = true;
                return new System.Uri("http://dead.com");
            }

            List<string> downloadEventArgs = new List<string>();
            var regex = Regex.Match(source, @"href=""downloads(\/.*\.zip)""");
            if (!regex.Success)
            {
                MessageBox.Show("Could not find the latest version of Metro! This program is not updated. Download the latest version or wait for it to be updated.");
            }
            LatestURI = new System.Uri("http://metroforsteam.com/downloads" + Convert.ToString(regex.Groups[1].Value));
            return LatestURI;
        }

        public static void InstallSkin(string steamDir)
        {
            bool customStylesExists = false;
            bool extrasFileExists = false;

            if (File.Exists(steamDir + SkinFolder + "\\custom.styles"))
            {
                customStylesExists = true;
                File.Copy(steamDir + SkinFolder + "\\custom.styles", Path.GetTempPath() + "custom.styles", true);
            }

            if (File.Exists(steamDir + SkinFolder + "\\extras.txt"))
            {
                extrasFileExists = true;
                File.Copy(steamDir + SkinFolder+"\\extras.txt", Path.GetTempPath() + "extras.txt", true);
            }

            if (Directory.Exists(steamDir + SkinFolder))
            {
                Directory.Delete(steamDir + SkinFolder, true);
            }

            using (ZipFile SteamSkin = ZipFile.Read(Path.GetTempPath() + "officialskin.zip"))
            { 
                SteamSkin.ExtractAll(Path.GetTempPath() + "\\MetroSkinTemp", ExtractExistingFileAction.OverwriteSilently);
            }
            string TempSkinDir = FindSkinDir(Path.GetTempPath() + "\\MetroSkinTemp");
            if (TempSkinDir == null)
            {
                TempSkinDir = Path.GetTempPath() + "\\MetroSkinTemp";
            }
            DirectoryCopy(TempSkinDir, steamDir + SkinFolder, true);
            if (customStylesExists)
            {
                File.Copy(Path.GetTempPath() + "custom.styles", steamDir + SkinFolder + "\\custom.styles", true);
            }

            if (extrasFileExists)
            {
                File.Copy(Path.GetTempPath() + "extras.txt", steamDir + SkinFolder + "\\extras.txt", true);
            }


        }
        private static string FindSkinDir(string DirectoryToLookIn)
        {
            string SkinDir = null;
            DirectoryInfo dir = new DirectoryInfo(DirectoryToLookIn);
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                if (subdir.Name.Contains("Metro"))
                {
                    SkinDir = subdir.FullName;
                    break;
                }
                else
                {
                    SkinDir = FindSkinDir(subdir.FullName);
                }
            }
            return SkinDir;
        }
        public static void InstallPatch(string steamDir)
        {
            DirectoryCopy(Path.GetTempPath() + "UPMetroSkin-installer\\normal_Unofficial Patch", steamDir + SkinFolder, true);
        }
        public static bool CheckSteamSkinDirectoryExists(string SteamDir)
        {
            return Directory.Exists(SteamDir);
        }

        public static void TempExtractPatch()
        {
            string path = Path.GetTempPath() + "installer.zip";

            if (File.Exists(path))
            {
                try
                {
                    using (ZipFile patchZip = ZipFile.Read(path))
                    {
                        patchZip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
                    }
                    err_ARCHIVE = false;
                }
                catch (Ionic.Zip.ZipException e)
                {
                    MessageBox.Show("Downloaded archive seems corrupt: " + e.Message);
                    err_ARCHIVE = true;
                }
            }
            else
            {
                MessageBox.Show("Missing archive:\n" + path);
                err_ARCHIVE = true;
            }
        }
        public static List<string> DetectExtras()
        {
            LocalData.GetExtras();
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
        public static void Cleanup()
        {
            Thread.Sleep(200);
            if (File.Exists(Path.GetTempPath() + "installer.zip")) { File.Delete(Path.GetTempPath() + "installer.zip"); }
            if (File.Exists(Path.GetTempPath() + "officialskin.zip")) { File.Delete(Path.GetTempPath() + "officialskin.zip"); }
            if (Directory.Exists(Path.GetTempPath() + "UPMetroSkin-installer")) { Directory.Delete(Path.GetTempPath() + "UPMetroSkin-installer", true); }
            if (Directory.Exists(Path.GetTempPath() + "MetroSkinTemp")) { Directory.Delete(Path.GetTempPath() + "MetroSkinTemp", true); }
        }
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static bool err_ARCHIVE = false;
    }
}
