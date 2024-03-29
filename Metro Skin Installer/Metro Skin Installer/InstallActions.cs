﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Ionic.Zip;

namespace Metro_Skin_Installer
{
    public class Release
    {
        public string Tag_Name { get; set; }
    }

    internal class InstallActions
    {

        public static bool workerRequestCancel = false;

        public const string SkinFolder = "\\" + "MetroSkin";
        public static void UpdateCheck()
        {
            var GitHubAPI = new WebClient();
            try
            {
                string jsonResponse = null;
                GitHubAPI.Headers.Add("user-agent", "MetroSkinInstaller");
                jsonResponse = GitHubAPI.DownloadString(@"https://api.github.com/repos/henrikx/metroskininstaller/releases");
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var ReleaseData = JsonSerializer.Deserialize<List<Release>>(jsonResponse, options).First();
                if (ReleaseData.Tag_Name != "v" + Application.ProductVersion)
                {
                    if (MessageBox.Show(null, "An update is available! Download now?", "Update", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        _ = Process.Start(
                            new ProcessStartInfo(@"https://github.com/henrikx/metroskininstaller/releases")
                            {
                                UseShellExecute = true
                            });
                    }
                }
            }
            catch (WebException)
            {
                _ = MessageBox.Show("Update check failed. Do you have Internet?");
            }
        }
        public static System.Uri GetLatestMetro(string branch = "master")
        {
            var LatestURI = $"https://codeload.github.com/redsigma/metro-for-steam/zip/{branch}";
            return new Uri(LatestURI);
        }

        public static void InstallSkin(string steamDir)
        {
            if (!Directory.Exists(steamDir))
            {
                _ = Directory.CreateDirectory(steamDir);
            }

            var customStylesExists = false;
            var extrasFileExists = false;

            if (File.Exists(steamDir + SkinFolder + "\\custom.styles"))
            {
                customStylesExists = true;
                File.Copy(steamDir + SkinFolder + "\\custom.styles", Path.GetTempPath() + "custom.styles", true);
            }

            if (File.Exists(steamDir + SkinFolder + "\\extras.txt"))
            {
                extrasFileExists = true;
                File.Copy(steamDir + SkinFolder + "\\extras.txt", Path.GetTempPath() + "extras.txt", true);
            }

            if (Directory.Exists(steamDir + SkinFolder))
            {
                Directory.Delete(steamDir + SkinFolder, true);
            }

            using (var SteamSkin = ZipFile.Read(Path.GetTempPath() + "officialskin.zip"))
            {
                SteamSkin.ExtractAll(Path.GetTempPath() + "\\MetroSkinTemp", ExtractExistingFileAction.OverwriteSilently);
            }
            var TempSkinDir = FindSkinDir(Path.GetTempPath() + "\\MetroSkinTemp");
            TempSkinDir ??= Path.GetTempPath() + "\\MetroSkinTemp";
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
            var dir = new DirectoryInfo(DirectoryToLookIn);
            var dirs = dir.GetDirectories();
            foreach (var subdir in dirs)
            {
                if (subdir.Name.Contains("Metro") || subdir.Name.Contains("metro"))
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

        public static event Action<int> ZipProgressChanged;

        public static void TempExtractPatch()
        {
            var path = Path.GetTempPath() + "installer.zip";

            if (File.Exists(path))
            {
                try
                {
                    using (var patchZip = ZipFile.Read(path))
                    {
                        patchZip.ExtractProgress += (s, e) =>
                        {
                            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry)
                            {
                                ZipProgressChanged?.Invoke(e.EntriesExtracted * 100 / e.EntriesTotal);
                            }

                            if (workerRequestCancel)
                            {
                                e.Cancel = true;
                            }
                        };
                        patchZip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
                    }
                    err_ARCHIVE = false;
                }
                catch (Ionic.Zip.ZipException e)
                {
                    _ = MessageBox.Show("Downloaded archive seems corrupt: " + e.Message);
                    err_ARCHIVE = true;
                }
            }
            else
            {
                _ = MessageBox.Show("Missing archive:\n" + path);
                err_ARCHIVE = true;
            }
        }
        public static List<string> DetectExtras()
        {
            var manifest = File.ReadAllLines(Path.GetTempPath() + "UPMetroSkin-installer\\manifest.txt");

            var ExtrasList = new List<string> { };
            for (var i = 0; i <= manifest.Length - 1; i++)
            {

                if (Regex.Match(manifest[i].Replace("\\", ""), "\"(.*?)\";\"(.*?)\";\"(.*?)\";\"(.*?)\"").Groups[3].Value == "")
                {
                    var getNameFromManifest = Regex.Match(manifest[i].Replace("\\", ""), "\"(.*?)\";\"(.*?)\";\"(.*?)\";\"(.*?)\"").Groups[1].Value;
                    ExtrasList.Add(getNameFromManifest);
                }
            }
            return ExtrasList;
        }
        public static void Cleanup()
        {
            Thread.Sleep(200);
            if (File.Exists(Path.GetTempPath() + "installer.zip"))
            {
                File.Delete(Path.GetTempPath() + "installer.zip");
            }
            if (File.Exists(Path.GetTempPath() + "officialskin.zip"))
            {
                File.Delete(Path.GetTempPath() + "officialskin.zip");
            }
            if (Directory.Exists(Path.GetTempPath() + "UPMetroSkin-installer"))
            {
                Directory.Delete(Path.GetTempPath() + "UPMetroSkin-installer", true);
            }
            if (Directory.Exists(Path.GetTempPath() + "MetroSkinTemp"))
            {
                Directory.Delete(Path.GetTempPath() + "MetroSkinTemp", true);
            }
        }
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                _ = Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                _ = file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (var subdir in dirs)
                {
                    var temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static bool err_ARCHIVE = false;
    }
}
