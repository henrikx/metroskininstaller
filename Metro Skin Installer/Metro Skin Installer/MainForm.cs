﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;

namespace Metro_Skin_Installer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

        }
        private void DownloadWorker_DoWork_1(object sender, DoWorkEventArgs e) //When installwindow is activated
        {
            DownloadOfficial(InstallActions.GetLatestMetro());
            InstallActions.InstallSkin();
            installProgress.Value += 25;
        }

        private void DownloadPatch_DoWork(object sender, DoWorkEventArgs e) //When select extras window is activated
        {
            DownloadPatch();
            progressBar1.Value = 100;
            InstallActions.TempExtractPatch();
            extrasListBox.DataSource = InstallActions.DetectExtras();
            progressBar1.Visible = false;
            PatchInstallButton.Enabled = true;
            PatchInstallButton.ForeColor = Color.White;
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

        string SteamSkinPath = InstallActions.FindSteamDir();

        #region Page1
        private void PatchedNextButton_Click(object sender, EventArgs e)
        {
            page1.Visible = false;
            page2patched.Visible = true;
            if (InstallActions.CheckSteamSkinDirectoryExists(SteamSkinPath))
            {
                DownloadPatchWorker.RunWorkerAsync();
            }

        }
        private void OfficialInstallbutton_Click(object sender, EventArgs e)
        {
            page1.Visible = false;
            InstallerPage.Visible = true;
            DownloadWorker.RunWorkerAsync();

        }
        private void PatchInstallButton_Click(object sender, EventArgs e)
        {
            InstallerPage.Visible = true;
            page2patched.Visible = false;
            DownloadWorker.RunWorkerAsync();
        }
        #endregion
        private void InstallerPageOpened(bool isPatch)
        {
            DownloadWorker.RunWorkerAsync();
        }

        private void DownloadPatch()
        {
            string TempDir = Path.GetTempPath();
            WebClient PatchDownloader = new WebClient();
            System.Uri uri = new System.Uri("https://github.com/redsigma/UPMetroSkin/archive/installer.zip");
            PatchDownloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(PatchDownloader_DownloadProgressChanged);
            PatchDownloader.DownloadFileAsync(uri, TempDir+"\\installer.zip");
            while (PatchDownloader.IsBusy) {  }

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
            Downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Downloader_DownloadProgressChanged);
            Downloader.DownloadFileAsync(URI, TempDir + "\\officialskin.zip");
            while (Downloader.IsBusy) { }
        }
        private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        { 
            installProgress.Value = e.ProgressPercentage/2;
        }

    }
}