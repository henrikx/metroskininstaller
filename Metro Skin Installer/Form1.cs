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

        private void button4_Click(object sender, EventArgs e)
        {
            string steamSkinPath = getSteamSkinPath();
            if(Directory.Exists (steamSkinPath))
            {
                richTextBox1.AppendText("\nSteam skin directory found: "+ steamSkinPath);
                WebClient downloadFile = new WebClient();
                richTextBox1.AppendText("\nLooking for latest version of Metro For Steam");
                string source = Convert.ToString(downloadFile.DownloadString("http://metroforsteam.com"));
                var regex = Regex.Match(source, "href=\"downloads(\\/*.*.zip)\"");
                if (Convert.ToString(regex) != "")
                {
                    string downloadUrl = "http://metroforsteam.com/downloads" + Convert.ToString(regex.Groups[1].Value);
                    richTextBox1.AppendText("\nFound latest version: " + Convert.ToString(downloadUrl));
                    downloadFileWorker.RunWorkerAsync();
                } else
                {
                    richTextBox1.AppendText("\nCouldn't find latest version on metroforsteam.com");
                }

            } else
            {
                richTextBox1.AppendText("\nCould not find a Steam installation");
            }

        }

        private void downloadFile_DoWork(object sender, DoWorkEventArgs e)
        {

        }
    }
}
