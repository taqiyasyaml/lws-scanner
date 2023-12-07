using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.Win32;
using System.IO;

namespace CobaScanner
{
    public partial class Form1 : Form
    {
        private static PreviewScannedImages scannedImages;
        private static DTWAINHelper DtwainHelper;
        private static WebSocketHelper webSocketHelper;
        private static ConfigHelper configHelper;
        private static bool AlreadyStartup = false;
        private static bool AlreadyShortcut = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void ResetProperties()
        {
            sourceBox.SelectedIndex = 0;
            paperBox.SelectedIndex = 0;
            colorBox.SelectedIndex = 0;
            resolutionInput.Value = 200;
            brightnessInput.Value = 0;
            brightnessLabel.Text = "0";
            contrastInput.Value = 0;
            contrastLabel.Text = "0";
        }

        private void brightnessInput_Scroll(object sender, EventArgs e)
        {
            this.brightnessLabel.Text = this.brightnessInput.Value.ToString();
        }

        private void startScanButton_Click(object sender, EventArgs e)
        {
            DtwainHelper.DoScan();
        }

        private void scannerGetButton_Click(object sender, EventArgs e)
        {
            DtwainHelper.DoGetScannerList();
        }

        private void scannerBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DtwainHelper.SetSelectedScanner();
        }

        private void contrastInput_Scroll(object sender, EventArgs e)
        {
            this.contrastLabel.Text = this.contrastInput.Value.ToString();
        }

        private void qualityInput_Scroll(object sender, EventArgs e)
        {
            this.qualityLabel.Text = this.qualityInput.Value.ToString();
        }

        private void stopScanButton_Click(object sender, EventArgs e)
        {
            DTWAINHelper.KeepScanning = false;
            this.stopScanButton.Enabled = false;
        }

        private void resetSettingButton_Click(object sender, EventArgs e)
        {
            DtwainHelper.ClearSelectedScanner(true);
            this.ResetProperties();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Environment.GetCommandLineArgs().Contains("/startup"))
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                this.Hide();
            }
            configHelper = new ConfigHelper(this, scannerBox, sourceBox, paperBox, colorBox, resolutionInput, brightnessInput, brightnessLabel, contrastInput, contrastLabel, qualityInput, qualityLabel, portInput, whiteListBox);
            scannedImages = new PreviewScannedImages(this, this.scanPicture, this.nextScanButton, this.previousScanButton, this.scanImagePositionLabel);
            DtwainHelper = new DTWAINHelper(scannedImages, configHelper, this, scannerBox, scannerLabel, sourceBox, paperBox, colorBox, resolutionInput, brightnessInput, brightnessLabel, contrastInput, contrastLabel, qualityInput, qualityLabel, stopScanButton);
            webSocketHelper = new WebSocketHelper(configHelper, this, this.portInput, this.startStopWSButton, this.whiteListBox, DtwainHelper);
            DtwainHelper.DoGetScannerList();
            webSocketHelper.StartWS();
            this.StartupButtonOnLoad();
            this.DesktopButtonOnLoad();
        }

        private void startStopWSButton_Click(object sender, EventArgs e)
        {
            webSocketHelper.StartStopWS();
        }

        private void deleteWhiteList_Click(object sender, EventArgs e)
        {
            for (int i = whiteListBox.SelectedIndices.Count - 1; i >= 0; i--)
            {
                whiteListBox.Items.RemoveAt(whiteListBox.SelectedIndices[i]);
            }
            if (Form1.configHelper != null)
            {
                Form1.configHelper.SaveWhiteList();
            }
        }

        private void saveSettingButton_Click(object sender, EventArgs e)
        {
            if (Form1.configHelper != null)
            {
                Form1.configHelper.SaveAllConfig(DTWAINHelper.SelectedScanner);
            }
        }

        private void StartupButtonOnLoad()
        {
            string CurrentFilename = "\"" + Process.GetCurrentProcess().MainModule!.FileName! + "\"";
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", false)!;
            string value = (string)key.GetValue(ConfigurationManager.AppSettings["StartUpID"]!)!;
            if (value != null && value.Equals(CurrentFilename + " /startup"))
            {
                Form1.AlreadyStartup = true;
                startupButton.Text = "Disable StartUp (Current User)";
            }
            else
            {
                Form1.AlreadyStartup = false;
                startupButton.Text = "Enable StartUp (Current User)";
            }
        }

        private void startupButton_Click(object sender, EventArgs e)
        {
            string CurrentFilename = "\"" + Process.GetCurrentProcess().MainModule!.FileName! + "\"";
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true)!;
            string value = (string)key.GetValue(ConfigurationManager.AppSettings["StartUpID"]!)!;
            if (Form1.AlreadyStartup)
            {
                if (value != null && value.Equals(CurrentFilename + " /startup"))
                {
                    key.DeleteValue(ConfigurationManager.AppSettings["StartUpID"]!);
                }
            }
            else
            {
                if (value != null)
                {
                    key.DeleteValue(ConfigurationManager.AppSettings["StartUpID"]!);
                }
                key.SetValue(ConfigurationManager.AppSettings["StartUpID"]!, CurrentFilename + " /startup");
            }
            this.StartupButtonOnLoad();
        }

        private void DesktopButtonOnLoad()
        {
            string DesktopPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                ConfigurationManager.AppSettings["ShortCutName"]! + ".lnk"
                );
            string MenuPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                ConfigurationManager.AppSettings["ShortCutName"]! + ".lnk"
                );
            Form1.AlreadyShortcut = File.Exists(DesktopPath) && File.Exists(MenuPath);
            if (Form1.AlreadyShortcut)
            {
                desktopButton.Text = "Delete Shortcut (Current User)";
            }
            else
            {
                desktopButton.Text = "Add Shortcut (Current User)";
            }
        }

        private void desktopButton_Click(object sender, EventArgs e)
        {
            string DesktopPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                ConfigurationManager.AppSettings["ShortCutName"]! + ".lnk"
                );
            string MenuPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                ConfigurationManager.AppSettings["ShortCutName"]! + ".lnk"
                );
            if (Form1.AlreadyShortcut)
            {
                if (File.Exists(DesktopPath))
                {
                    File.Delete(DesktopPath);
                }
                if (File.Exists(MenuPath))
                {
                    File.Delete(MenuPath);
                }
            }
            else
            {
                string CurrentFilename = Process.GetCurrentProcess().MainModule!.FileName!;
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                if (File.Exists(DesktopPath))
                {
                    File.Delete(DesktopPath);
                }
                IWshRuntimeLibrary.WshShortcut DesktopShortcut = (IWshRuntimeLibrary.WshShortcut)shell.CreateShortcut(DesktopPath);
                DesktopShortcut.TargetPath = CurrentFilename;
                DesktopShortcut.Save();
                if (File.Exists(MenuPath))
                {
                    File.Delete(MenuPath);
                }
                IWshRuntimeLibrary.WshShortcut MenuShortcut = (IWshRuntimeLibrary.WshShortcut)shell.CreateShortcut(MenuPath);
                MenuShortcut.TargetPath = CurrentFilename;
                MenuShortcut.Save();
            }
            this.DesktopButtonOnLoad();
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Do you want to close this application instead of minimized it?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
        }

        private void nextScanButton_Click(object sender, EventArgs e)
        {
            Form1.scannedImages.NextImageOnClick();
        }

        private void previousScanButton_Click(object sender, EventArgs e)
        {
            Form1.scannedImages.PreviousImageOnClick();
        }
    }
}