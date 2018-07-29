using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Metro_Skin_Installer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            if (!hasPermission(FindSteamSkinDir()))
            {
                Environment.Exit(0);
            }
        }

        public static bool hasPermission(string dir)
        {
            try
            {
                File.CreateText(dir + "\\chkPerm").Close();
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("{0} Exception caught.", e);
                MessageBox.Show("Access to " + dir + " is denied. Please open the app with admin rights.",
                    System.Reflection.Assembly.GetCallingAssembly().GetName().Name,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return false;
            }

            File.Delete(dir + "\\chkPerm");
            return true;
        }


        private void DownloadWorker_DoWork_1(object sender, DoWorkEventArgs e) //When installwindow is activated
        {
            CurrentWorker.Text = "Base Skin";
            List<bool> InstallerArguments = e.Argument as List<bool>;

            DownloadOfficial(InstallActions.GetLatestMetro());

            if (InstallActions.err_ARCHIVE)
            {
                page1.Visible = true;
                InstallerPage.Visible = false;
                return;
            }
            
            InstallActions.InstallSkin(FindSteamSkinDir());

            installProgress.Value += 5;
            if (InstallerArguments[0])
            {
                CurrentWorker.Text = "Unofficial Patch";
                InstallActions.InstallPatch(FindSteamSkinDir());
                installProgress.Value += 25;
                InstallExtras();
            }
            CurrentWorker.Text = "Cleaning Up";
            InstallActions.Cleanup();
            installProgress.Value = 100;
            CurrentWorker.Text = "Finished";
            button1.Enabled = true;
            button1.ForeColor = Color.White;
            button1.Image = Properties.Resources.close_button;
        }
        private void InstallExtras()
        {
            if (extrasListBox.CheckedItems.Count >= 1)
            {
                int incrementalProgressbarIncrease = 20 / extrasListBox.CheckedItems.Count;
                for (int i = 0; i < extrasListBox.Items.Count; i++)
                {
                    if (extrasListBox.GetItemChecked(i))
                    {
                        string[] manifest = File.ReadAllLines(Path.GetTempPath() + "\\UPMetroSkin-installer\\manifest.txt");
                        string ExtraPath = Regex.Match((manifest[i].Replace("\\", "")), "\"(.*?)\";\"(.*?)\";\"(.*?)\";\"(.*?)\"").Groups[2].Value;
                        CurrentWorker.Text = extrasListBox.GetItemText(extrasListBox.Items[i]);
                        InstallActions.DirectoryCopy(Path.GetTempPath() + "\\UPMetroSkin-installer\\normal_Extras\\" + ExtraPath, FindSteamSkinDir() + "\\Metro 4.2.4", true);
                        installProgress.Value += incrementalProgressbarIncrease;
                    }
                }
            }

        }
        private void DownloadPatch_DoWork(object sender, DoWorkEventArgs e) //When select extras window is activated
        {
            DownloadPatch();
            progressBar1.Value = 100;
            InstallActions.TempExtractPatch();

            if (InstallActions.err_ARCHIVE)
            {
                page1.Visible = true;
                page2patched.Visible = false;
                return;
            }
            
            extrasListBox.DataSource = InstallActions.DetectExtras();
            progressBar1.Visible = false;
            extrasLoadingText.Visible = false;
            PatchInstallButton.Enabled = true;
            PatchInstallButton.ForeColor = Color.White;
            PatchInstallButton.Image = Properties.Resources.right_arrow;
        }
        private void ExitButton_Click(object sender, EventArgs e)
        {
            InstallActions.Cleanup();
            Application.Exit();
        }
        #region dragabbletitlebar
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private void FormMain_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void FormMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void FormMain_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
        #endregion

        string SteamSkinPath = FindSteamSkinDir();

        public static string FindSteamSkinDir()
        {
            using (var registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam"))
            {
                string filePath = null;
                var regFilePath = registryKey?.GetValue("SteamPath");
                if (regFilePath != null)
                {
                    filePath = System.IO.Path.Combine(regFilePath.ToString().Replace(@"/", @"\"), "skins");
                }
                return filePath;
            }
        }

        #region Page1
        private void PatchedNextButton_Click(object sender, EventArgs e)
        {
            page1.Visible = false;
            page2patched.Visible = true;
            if (InstallActions.CheckSteamSkinDirectoryExists(SteamSkinPath))
            {
                DownloadPatchWorker.RunWorkerAsync();
            } else
            {
               MessageBox.Show("No Steam Skin directory found.");
            }

        }
        private void OfficialInstallbutton_Click(object sender, EventArgs e)
        {
            if (InstallActions.CheckSteamSkinDirectoryExists(FindSteamSkinDir()))
            {
                bool isPatch = false;
                List<bool> InstallerArguments = new List<bool>();
                InstallerArguments.Add(isPatch);
                page1.Visible = false;
                InstallerPage.Visible = true;
                DownloadWorker.RunWorkerAsync(InstallerArguments);
            } else
            {
                MessageBox.Show("No Steam Skin directory found.");
            }
        }
        private void PatchInstallButton_Click(object sender, EventArgs e)
        {
            bool isPatch = true;
            List<bool> InstallerArguments = new List<bool>();
            InstallerArguments.Add(isPatch);
            InstallerPage.Visible = true;
            page2patched.Visible = false;
            DownloadWorker.RunWorkerAsync(InstallerArguments);
        }
        #endregion
        private void DownloadPatch()
        {
            string TempDir = Path.GetTempPath();
            WebClient PatchDownloader = new WebClient();

            System.Uri uri = new System.Uri("https://github.com/redsigma/UPMetroSkin/archive/installer.zip");

            DownloadProgressChangedEventHandler progressHandler = new DownloadProgressChangedEventHandler(PatchDownloader_DownloadProgressChanged);
            PatchDownloader.DownloadProgressChanged += progressHandler;

            PatchDownloader.DownloadFileAsync(uri, TempDir + "\\installer.zip");
            while (PatchDownloader.IsBusy)
            {
               Thread.Sleep(500);
            }
        }
        private void PatchDownloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if(e.ProgressPercentage <= 95)
            {
                progressBar1.Value = e.ProgressPercentage;
            }
        }

 
        private void DownloadOfficial(System.Uri URI)
        {
            string TempDir = Path.GetTempPath();
            WebClient Downloader = new WebClient();
 
            DownloadProgressChangedEventHandler progressHandler = new DownloadProgressChangedEventHandler(Downloader_DownloadProgressChanged);
            Downloader.DownloadProgressChanged += progressHandler;

            Downloader.DownloadFileAsync(URI, TempDir + "\\officialskin.zip");
            while (Downloader.IsBusy)
            {
                Thread.Sleep(500);
            }
        }
        private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        { 
            installProgress.Value = e.ProgressPercentage/2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
