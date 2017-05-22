using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ionic.Zip;
using System.Net;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace Metro_Skin_Installer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                noPatch.Visible = true;
            }
            else if (radioButton2.Checked)
            {
                richTextBox1.AppendText("\nDownloading latest community patch");
                List<String> downloadPatchEventArgs = new List<string>();
                downloadPatchEventArgs.Add("https://github.com/redsigma/UPMetroSkin/archive/installer.zip"); //From where to download patch
                if (!Directory.Exists(Path.GetTempPath() + "patchfiles\\"))
                {
                    Directory.CreateDirectory(Path.GetTempPath() + "patchfiles\\");
                }
                downloadPatchEventArgs.Add(Path.GetTempPath() + "patchfiles\\"); //Where to extract temporary files
                downloadPatchEventArgs.Add(Path.GetTempPath() + "patchfiles.zip"); //Where temporary file is downloaded
                downloadPatchEventArgs.Add("false");
                downloadFileWorker.RunWorkerAsync(downloadPatchEventArgs);
                withPatch.Visible = true;


            }
            else
            {
                MessageBox.Show("Please select a version!");
            }

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            noPatch.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            withPatch.Visible = false;
            checkedListBox1.Items.Clear();
        }

        public string getSteamSkinPath()
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
        private void downloadStarter(bool debug, bool isPatch)
        {
            string steamSkinPath = getSteamSkinPath();
            if (Directory.Exists(steamSkinPath))
            {
                richTextBox1.AppendText("\nSteam skin directory found: " + steamSkinPath);
                WebClient downloadFile = new WebClient();
                richTextBox1.AppendText("\nLooking for latest version of Metro For Steam");
                string source = Convert.ToString(downloadFile.DownloadString("http://metroforsteam.com"));
                List<string> downloadEventArgs = new List<string>();
                var regex = Regex.Match(source, "href=\"downloads(\\/*.*.zip)\""); //This regexp to find the newest version, is subject to change because it only supports finding it if the file has 3 version numbers.

                if (Convert.ToString(regex) != "")
                {
                    downloadEventArgs.Add("http://metroforsteam.com/downloads" + Convert.ToString(regex.Groups[1].Value)); //Where to download skin from
                    downloadEventArgs.Add(steamSkinPath); //Where to install skin
                    downloadEventArgs.Add(Path.GetTempPath() + Convert.ToString(regex.Groups[1].Value)); //Where temporarily downloaded file is located
                    downloadEventArgs.Add(Convert.ToString(isPatch));
                    richTextBox1.AppendText("\nFound latest version: " + Convert.ToString(downloadEventArgs[0]));
                    downloadFileWorker.RunWorkerAsync(downloadEventArgs);
                }
                else
                {
                    richTextBox1.AppendText("\nCouldn't find latest version on metroforsteam.com");
                }

            }
            else if (debug)
            {
                WebClient downloadFile = new WebClient();
                string source = Convert.ToString(downloadFile.DownloadString("http://metroforsteam.com"));
                var regex = Regex.Match(source, "href=\"downloads(\\/*.*.zip)\"");
                string debugpath = Application.StartupPath;
                string debugFilePath = Application.StartupPath + Convert.ToString(regex.Groups[1].Value);
                richTextBox1.AppendText("\nSteam skin directory found: (debug mode) " + debugpath);
                richTextBox1.AppendText("\nLooking for latest version of Metro For Steam");
                if (Convert.ToString(regex) != "")
                {
                    string downloadUrl = "http://metroforsteam.com/downloads" + Convert.ToString(regex.Groups[1].Value);
                    richTextBox1.AppendText("\nFound latest version: " + Convert.ToString(downloadUrl));
                    List<string> downloaderEventArgs = new List<string>();
                    downloaderEventArgs.Add(downloadUrl);
                    downloaderEventArgs.Add(debugpath);
                    downloaderEventArgs.Add(debugFilePath);
                    downloaderEventArgs.Add(Convert.ToString(isPatch));
                    downloadFileWorker.RunWorkerAsync(downloaderEventArgs);
                }
                else
                {
                    richTextBox1.AppendText("\nCouldn't find latest version on metroforsteam.com");
                }
            }
            else
            {
                richTextBox1.AppendText("\nCould not find a Steam installation");
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            bool debug = false;
            bool isPatch = false;
            downloadStarter(debug, isPatch);

        }

        private void downloadFile_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> DownloaderEventArgs = e.Argument as List<string>;
            if (DownloaderEventArgs[2].Contains("patchfiles") && Directory.Exists(DownloaderEventArgs[1] + "UPMetroSkin\\"))
            {
                detectExtras(DownloaderEventArgs[1] + "UPMetroSkin-installer\\");
                return;
            }
            WebClient downloadFile = new WebClient();

            richTextBox1.AppendText("\nDownloading file...");
            downloadFile.DownloadProgressChanged += (s, f) =>
            {
                richTextBox1.AppendText("\n" + f.ProgressPercentage);
            };
            downloadFile.DownloadFile(DownloaderEventArgs[0], DownloaderEventArgs[2]);
            UnZipfile(DownloaderEventArgs[1], DownloaderEventArgs[2], DownloaderEventArgs[3]);
        }
        private void UnZipfile(string steamDir, string path, string isPatch)
        {

            using (ZipFile zip1 = Ionic.Zip.ZipFile.Read(path))
            {
                richTextBox1.AppendText("\nExtracting...");
                zip1.ExtractAll(steamDir, ExtractExistingFileAction.OverwriteSilently);
                richTextBox1.AppendText("\nExtracted");
                if (isPatch == "True")
                {
                    InstallPatch(steamDir);
                }
            }
            if (path.Contains("patchfiles"))
            {
                button5.Enabled = true;
                detectExtras(steamDir + "UPMetroSkin-installer\\");
                richTextBox1.AppendText("\nPatch downloaded. Select optional extras and press \"Next\" to start the install");
            }
            File.Delete(path);
        }
        private void detectExtras(string extrasPath)
        {
            string[] manifest = File.ReadAllLines(extrasPath + "\\manifest.txt");

            for (int i = 0; i <= manifest.Length - 1; i++)
            {

                if (Regex.Match((manifest[i].Replace("\\", "")), "\"(.*?)\";\"(.*?)\";\"(.*?)\";\"(.*?)\"").Groups[3].Value == "")
                {
                    string getNameFromManifest = Regex.Match((manifest[i].Replace("\\", "")), "\"(.*?)\";\"(.*?)\";\"(.*?)\";\"(.*?)\"").Groups[1].Value;
                    checkedListBox1.Items.Add(getNameFromManifest);

                }

            }
        }
        private void InstallPatch(string steamDir)
        {
            richTextBox1.AppendText("\nInstalling patch");
            DirectoryCopy(Path.GetTempPath() + "patchfiles\\UPMetroSkin-installer\\normal_Unofficial Patch", steamDir+"\\Metro 4.2.4", true);
            for (int checkCheckedNum = 0; checkCheckedNum <= checkedListBox1.Items.Count-1; checkCheckedNum++)
            {
                if (checkedListBox1.GetItemChecked(checkCheckedNum))
                {

                }
            }
            richTextBox1.AppendText("\nAll done!");

        }

        private void button5_Click(object sender, EventArgs e)
        {
            bool debug = false;
            bool isPatch = true;
            downloadStarter(debug, isPatch);
        }
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
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


    }
}
