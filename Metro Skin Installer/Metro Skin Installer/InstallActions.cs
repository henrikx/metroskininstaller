using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Ionic.Zip;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Metro_Skin_Installer
{
    class InstallActions
    {
        public static Uri GetLatestMetro()
        {
            Uri LatestURI = new Uri("https://google.com/");
            WebClient downloadFile = new WebClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            string source = "";
            try
            {
                source = Convert.ToString(downloadFile.DownloadString("http://metroforsteam.com"));
                err_ARCHIVE = false;
            }
            catch (WebException e)
            {
                MessageBox.Show(e.Message); // No internet
                err_ARCHIVE = true;
                return new Uri("http://dead.com");
            }

            List<string> downloadEventArgs = new List<string>();
            var regex = Regex.Match(source, @"href=""downloads(\/.*\.zip)""");
            if (!regex.Success)
            {
                MessageBox.Show("Could not find the latest version of Metro! This program is not updated. Download the latest version or wait for it to be updated.");
            }
            LatestURI = new Uri("http://metroforsteam.com/downloads" + Convert.ToString(regex.Groups[1].Value));
            return LatestURI;
        }

        public static void InstallSkin(string steamDir)
        {
            bool customStylesExists = false;
            bool extrasFileExists = false;

            if (File.Exists(steamDir + "\\Metro\\custom.styles"))
            {
                customStylesExists = true;
                File.Copy(steamDir + "\\Metro\\custom.styles", Path.GetTempPath() + "custom.styles", true);
            }

            if (File.Exists(steamDir + "\\Metro\\extras.txt"))
            {
                extrasFileExists = true;
                File.Copy(steamDir + "\\Metro\\extras.txt", Path.GetTempPath() + "extras.txt", true);
            }

            if (Directory.Exists(steamDir + "\\Metro"))
            {
                Directory.Delete(steamDir + "\\Metro", true);
            }

            using (ZipFile SteamSkin = ZipFile.Read(Path.GetTempPath() + "officialskin.zip"))
            {
                SteamSkin.ExtractAll(steamDir + "\\Metro\\", ExtractExistingFileAction.OverwriteSilently);
            }

            if (customStylesExists)
            {
                File.Copy(Path.GetTempPath() + "custom.styles", steamDir + "\\Metro\\custom.styles", true);
            }

            if (extrasFileExists)
            {
                File.Copy(Path.GetTempPath() + "extras.txt", steamDir + "\\Metro\\extras.txt", true);
            }


        }
        public static void InstallPatch(string steamDir)
        {
            DirectoryCopy(Path.GetTempPath() + "UPMetroSkin-installer\\normal_Unofficial Patch", steamDir + "\\Metro", true);
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
                catch (ZipException e)
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
            if (File.Exists(Path.GetTempPath() + "installer.zip")) { File.Delete(Path.GetTempPath() + "installer.zip"); }
            if (File.Exists(Path.GetTempPath() + "officialskin.zip")) { File.Delete(Path.GetTempPath() + "officialskin.zip"); }
            if (Directory.Exists(Path.GetTempPath() + "UPMetroSkin-installer")) { Directory.Delete(Path.GetTempPath() + "UPMetroSkin-installer", true); }
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
