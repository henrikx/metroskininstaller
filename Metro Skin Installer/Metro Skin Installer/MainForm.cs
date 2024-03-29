﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Metro_Skin_Installer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            FormClosing += formClosingHandler;
            var UpdateCheck = new Thread(InstallActions.UpdateCheck);
            UpdateCheck.Start();
            if (SteamSkinPath == null)
            {
                _ = MessageBox.Show("Steam couldn't be found. \nPlease install steam or reinstall if already installed.");
                Environment.Exit(0);
            }
            if (!hasPermission(SteamSkinPath))
            {
                Environment.Exit(0);
            }
        }

        private void formClosingHandler(object sender, CancelEventArgs e)
        {
            InstallActions.workerRequestCancel = true;
            if (DownloadPatchWorker.IsBusy)
            {
                DownloadPatchWorker.CancelAsync();
            }
            while (DownloadPatchWorker.IsBusy || DownloadWorker.IsBusy)
            {
                Application.DoEvents();
            };
        }
        public static bool hasPermission(string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                {
                    _ = Directory.CreateDirectory(dir);
                }
                File.CreateText(dir + "\\chkPerm").Close();
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("{0} Exception caught.", e);
                _ = MessageBox.Show("Access to " + dir + " is denied. Please open the app with admin rights.",
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
            var InstallerArguments = e.Argument as List<bool>;
            try
            {
                DownloadOfficialAsync(InstallActions.GetLatestMetro("vanilla")).Wait();
            }
            catch (AggregateException)
            {
                DownloadOfficialAsync(InstallActions.GetLatestMetro()).Wait(); // if vanilla branch not found then download from master branch
            }

            if (InstallActions.err_ARCHIVE)
            {
                page1.Visible = true;
                InstallerPage.Visible = false;
                return;
            }

            InstallActions.InstallSkin(SteamSkinPath);

            installProgress.Value += 5;
            if (InstallerArguments[0])
            {
                CurrentWorker.Text = "Unofficial Patch";
                InstallActions.InstallPatch(SteamSkinPath);
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
                var incrementalProgressbarIncrease = 20 / extrasListBox.CheckedItems.Count;
                var manifest = File.ReadAllLines(Path.GetTempPath() + "\\UPMetroSkin-installer\\manifest.txt");
                var checkedExtras = new List<string>();
                for (var i = 0; i < extrasListBox.Items.Count; i++)
                {
                    if (extrasListBox.GetItemChecked(i))
                    {
                        var ExtraPath = Regex.Match(manifest[i].Replace("\\", ""), "\"(.*?)\";\"(.*?)\";\"(.*?)\";\"(.*?)\"").Groups[2].Value;
                        CurrentWorker.Text = extrasListBox.GetItemText(extrasListBox.Items[i]);
                        checkedExtras.Add(CurrentWorker.Text);
                        InstallActions.DirectoryCopy(Path.GetTempPath() + "\\UPMetroSkin-installer\\normal_Extras\\" + ExtraPath, SteamSkinPath + InstallActions.SkinFolder, true);
                        installProgress.Value += incrementalProgressbarIncrease;
                    }
                }
                if (saveExtrasCheckBox.Checked)
                {
                    File.WriteAllLines(SteamSkinPath + InstallActions.SkinFolder + "\\extras.txt", checkedExtras);
                }
            }
            else
            {
                if (File.Exists(SteamSkinPath + InstallActions.SkinFolder + "\\extras.txt"))
                {
                    File.Delete(SteamSkinPath + InstallActions.SkinFolder + "\\extras.txt");
                }
            }
        }

        private void DownloadPatch_DoWork(object sender, DoWorkEventArgs e) //When select extras window is activated
        {
            DownloadPatch();
            if (DownloadPatchWorker.CancellationPending)
            {
                InstallActions.Cleanup();
                return;
            }
            progressBar1.Value = 100;
            InstallActions.ZipProgressChanged += (f) => { progressBar1.Value = f; };
            InstallActions.TempExtractPatch();
            InstallActions.ZipProgressChanged -= (f) => { progressBar1.Value = f; };
            if (!DownloadPatchWorker.CancellationPending)
            {
                if (InstallActions.err_ARCHIVE)
                {
                    page1.Visible = true;
                    page2patched.Visible = false;
                    return;
                }

                extrasListBox.DataSource = InstallActions.DetectExtras();
                if (File.Exists(SteamSkinPath + InstallActions.SkinFolder + "\\extras.txt"))
                {
                    var savedExtras = File.ReadAllLines(SteamSkinPath + InstallActions.SkinFolder + "\\extras.txt");
                    for (var i = 0; i < extrasListBox.Items.Count; i++)
                    {
                        if (savedExtras.Contains(extrasListBox.Items[i]))
                        {
                            extrasListBox.SetItemChecked(i, true);
                        }
                    }
                    if (savedExtras.Length > 0)
                    {
                        saveExtrasCheckBox.Checked = true;
                    }
                }
                progressBar1.Visible = false;
                extrasLoadingText.Visible = false;
                _ = saveExtrasCheckBox.Invoke((MethodInvoker)(() => { saveExtrasCheckBox.Visible = true; }));
                PatchInstallButton.Enabled = true;
                PatchInstallButton.ForeColor = Color.White;
                PatchInstallButton.Image = Properties.Resources.right_arrow;
            }
            else
            {
                InstallActions.Cleanup();
            }
        }
        private void ExitButton_Click(object sender, EventArgs e)
        {
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
            dragFormPoint = Location;
        }

        private void FormMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                var dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void FormMain_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
        #endregion

        private readonly string SteamSkinPath = FindSteamSkinDir();

        public static string FindSteamSkinDir()
        {
            using var registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam");
            string filePath = null;
            var regFilePath = registryKey?.GetValue("SteamPath");
            if (regFilePath != null)
            {
                filePath = System.IO.Path.Combine(regFilePath.ToString().Replace(@"/", @"\"), "skins");
            }
            return filePath;
        }

        #region Page1
        private void PatchedNextButton_Click(object sender, EventArgs e)
        {
            page1.Visible = false;
            page2patched.Visible = true;
            if (InstallActions.CheckSteamSkinDirectoryExists(SteamSkinPath))
            {
                DownloadPatchWorker.RunWorkerAsync();
            }
            else
            {
                _ = MessageBox.Show("No Steam Skin directory found.");
            }
        }
        private void OfficialInstallbutton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("You have selected to install an unsupported version of the skin! This version is UNMAINTAINED, contains MAJOR bugs and is NOT recommended. Press OK if you know what you're doing and you want to proceed.", "Confirmation", MessageBoxButtons.OKCancel) != DialogResult.OK)
            {
                return;
            }
            if (InstallActions.CheckSteamSkinDirectoryExists(SteamSkinPath))
            {
                var isPatch = false;
                var InstallerArguments = new List<bool>
                {
                    isPatch
                };
                page1.Visible = false;
                InstallerPage.Visible = true;
                DownloadWorker.RunWorkerAsync(InstallerArguments);
            }
            else
            {
                _ = MessageBox.Show("No Steam Skin directory found.");
            }
        }
        private void PatchInstallButton_Click(object sender, EventArgs e)
        {
            var isPatch = true;
            var InstallerArguments = new List<bool>
            {
                isPatch
            };
            InstallerPage.Visible = true;
            page2patched.Visible = false;
            ExitButton.Enabled = false;
            DownloadWorker.RunWorkerAsync(InstallerArguments);
        }
        #endregion
        private void DownloadPatch()
        {
            var TempDir = Path.GetTempPath();
            var PatchDownloader = new WebClient();

            var uri = new System.Uri("https://github.com/redsigma/UPMetroSkin/archive/installer.zip");

            PatchDownloader.DownloadProgressChanged += (s, e) =>
            {
                if (e.ProgressPercentage <= 95)
                {
                    progressBar1.Value = e.ProgressPercentage;
                }
            };

            PatchDownloader.DownloadFileAsync(uri, TempDir + "\\installer.zip");
            while (PatchDownloader.IsBusy)
            {
                if (DownloadPatchWorker.CancellationPending)
                {
                    PatchDownloader.CancelAsync();
                }
                Thread.Sleep(500);
            }
        }

        private async Task DownloadOfficialAsync(System.Uri URI)
        {
            var TempDir = Path.GetTempPath();
            var Downloader = new WebClient();

            var progressHandler = new DownloadProgressChangedEventHandler(Downloader_DownloadProgressChanged);
            Downloader.DownloadProgressChanged += progressHandler;

            await Downloader.DownloadFileTaskAsync(URI, TempDir + "\\officialskin.zip");

        }
        private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            installProgress.Value = e.ProgressPercentage / 2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
