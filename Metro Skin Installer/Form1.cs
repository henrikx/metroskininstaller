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
        }

        public string getSteamSkinPath()
        {
            using (var registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam"))
            {
                string filePath = null;
                var regFilePath = registryKey?.GetValue("SteamPath");
                if (regFilePath != null)
                {
                    filePath = Path.Combine(regFilePath.ToString().Replace(@"/",@"\"), "skins");
                }
                return filePath;
            }

            
        }
        private void downloadStarter(bool debug)
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
                    downloadEventArgs.Add(Path.GetTempPath()+Convert.ToString(regex.Groups[1].Value)); //Where temporarily downloaded file is located
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
                    downloaderEventArgs.Add("true");
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
            downloadStarter(debug);

        }

        private void downloadFile_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> DownloaderEventArgs = e.Argument as List<string>;
            if (DownloaderEventArgs[2].Contains("patchfiles") && Directory.Exists(DownloaderEventArgs[1]))
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
            downloadFile.DownloadFile(DownloaderEventArgs[0],DownloaderEventArgs[2]);
            UnZipfile(DownloaderEventArgs[1],DownloaderEventArgs[2]);
        }
        private void UnZipfile(string steamDir, string path)
        {

            using (ZipFile zip1 = Ionic.Zip.ZipFile.Read(path))
            {
                richTextBox1.AppendText("\nExtracting...");
                zip1.ExtractAll(steamDir, ExtractExistingFileAction.OverwriteSilently);
            }
            if (path.Contains("patchfiles"))
            {
                button5.Enabled = true;
                detectExtras(steamDir+ "UPMetroSkin-installer\\normal_Extras\\");
                richTextBox1.AppendText("\nPatch downloaded. Select optional extras and press \"Next\" to start the install");
            }
            File.Delete(path);
        }
        private void detectExtras(string extrasPath)
        {
            string[] manifest = File.ReadAllLines(extrasPath + "\\manifest.txt");
            
            for (int i=0; i <= manifest.Length-1; i++)
            {
                string getNameFromManifest = Regex.Match((manifest[i].Replace("\\","")), "\"(.*?)\"").Groups[1].Value;
                checkedListBox1.Items.Add(getNameFromManifest);
            }
        }
    }
}
