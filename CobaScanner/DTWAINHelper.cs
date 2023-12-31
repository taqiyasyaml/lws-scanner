﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Diagnostics;

/*  Use this for 32-bit compilation */
using Dynarithmic32;
using DTWAIN_SOURCE = System.IntPtr;
using DTWAIN_ARRAY = System.IntPtr;
using DTWAIN_RANGE = System.IntPtr;
using DTWAIN_FRAME = System.IntPtr;
using DTWAIN_PDFTEXTELEMENT = System.IntPtr;
using DTWAIN_HANDLE = System.IntPtr;
using DTWAIN_IDENTITY = System.IntPtr;
using DTWAIN_OCRENGINE = System.IntPtr;
using DTWAIN_OCRTEXTINFOHANDLE = System.IntPtr;
using TW_UINT16 = System.UInt16;
using TW_UINT32 = System.UInt32;
using TW_BOOL = System.UInt16;
using DTWAIN_MEMORY_PTR = System.IntPtr;

/*  Use this for 64-bit compilation */
/*using Dynarithmic64;
using DTWAIN_SOURCE = System.IntPtr;
using DTWAIN_ARRAY = System.IntPtr;
using DTWAIN_RANGE = System.IntPtr;
using DTWAIN_FRAME = System.IntPtr;
using DTWAIN_PDFTEXTELEMENT = System.IntPtr;
using DTWAIN_HANDLE = System.IntPtr;
using DTWAIN_IDENTITY = System.IntPtr;
using DTWAIN_OCRENGINE = System.IntPtr;
using DTWAIN_OCRTEXTINFOHANDLE = System.IntPtr;
using TW_UINT16 = System.UInt16;
using TW_UINT32 = System.UInt32;
using TW_BOOL = System.UInt16;
using DTWAIN_MEMORY_PTR = System.IntPtr;*/

using static System.Runtime.CompilerServices.RuntimeHelpers;
using Fleck;
using System.Text.Json.Nodes;
using System.Runtime.InteropServices;

namespace CobaScanner
{
    internal class DTWAINHelper
    {
        private PreviewScannedImages ViewScanImages;
        private ConfigHelper Conf;

        private Form1 form1;
        private Form2? formStatus = null;
        private ListBox scannerBox;
        private Label scannerLabel;
        private ComboBox sourceBox;
        private ComboBox paperBox;
        private ComboBox colorBox;
        private NumericUpDown resolutionInput;
        private TrackBar brightnessInput;
        private Label brightnessLabel;
        private TrackBar contrastInput;
        private Label contrastLabel;
        private NumericUpDown maxAcqInput;
        private NumericUpDown maxPageInput;
        private CheckBox sourceUICheckBox;
        private CheckBox sourceIndicatorCheckBox;
        private TrackBar qualityInput;
        private Label qualityLabel;
        private Button stopScanButton;

        public static bool IsDTWAINBusy = false;
        public static string SelectedScanner = "";
        public static bool KeepScanning = false;
        public static IWebSocketConnection? socket = null;

        private BackgroundWorker DoScanWorker = new BackgroundWorker();
        private BackgroundWorker DoGetScanWorker = new BackgroundWorker();
        private BackgroundWorker DoSetScanWorker = new BackgroundWorker();

        public static readonly int[] PaperInts = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 1, 22, 23, 24, 25, 26, 27, 28, 12, 6, 29, 7, 30, 31, 32, 33, 34, 35, 36, 37, 38, 2, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53 };
        public static readonly int[] ColorInts = new int[] { 1000, 0, 1, 2, 5 };
        private static DTWAIN_HANDLE DTWAINThread;

        private static readonly bool NeedSaveLogFile = true;
        private static String SaveAppLogFile = "";
        private static String SaveDTwainLogFile = "";

        public static bool StartDTWAIN()
        {
            if (TwainAPI.DTWAIN_IsTwainAvailable() != 1)
            {
                MessageBox.Show("DTWAIN_NOT_FOUND", "ERROR!");
                return false;
            }
            TwainAPI.DTWAIN_UseMultipleThreads(1);
            DTWAINHelper.DTWAINThread = TwainAPI.DTWAIN_SysInitialize();
            if(DTWAINHelper.NeedSaveLogFile)
            {
                string FileLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string FolderLocation = System.IO.Path.Combine(Path.GetDirectoryName(FileLocation)!, "logs");
                if (!Directory.Exists(FolderLocation))
                {
                    Directory.CreateDirectory(FolderLocation);
                }
                DTWAINHelper.SaveAppLogFile = System.IO.Path.Combine(FolderLocation, DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-app.log.txt");
                DTWAINHelper.SaveDTwainLogFile = System.IO.Path.Combine(FolderLocation, DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-dtwain.log.txt");
                TwainAPI.DTWAIN_SetTwainLog(TwainAPI.DTWAIN_LOG_FILEAPPEND, DTWAINHelper.SaveDTwainLogFile);
            }
            return true;
        }

        public static bool StopDTWAIN()
        {
            Stopwatch S = Stopwatch.StartNew();
            int ResultDestroy = 0;
            TwainAPI.DTWAIN_StartThread(DTWAINHelper.DTWAINThread);
            do
            {
                ResultDestroy = TwainAPI.DTWAIN_SysDestroy();
            } while (ResultDestroy == 0 && S.ElapsedMilliseconds < 60_000);
            TwainAPI.DTWAIN_EndThread(DTWAINHelper.DTWAINThread);
            return ResultDestroy == 1;
        }

        private class DoScanArguments
        {
            public int Source = -1;
            public int Paper = 0;
            public int Color = 0;
            public int Resolution = 200;
            public int Brightness = 0;
            public int Contrast = 0;
            public int MaxAcq = 0;
            public int MaxPage = 0;
            public bool SourceUI = false;
            public bool SourceIndicator = false;
            public bool SourceIndicatorEnable = false;
            public long Quality = 100;
        }
        private class ScannerProperties
        {
            public string ProductName;
            public bool IsSupportFeeder;
            public bool IsSupportDuplex;
            public bool IsSupportIndicator;
        }
        private class DoGetScanResult
        {
            public List<String> Scanners;
            public ScannerProperties? SelectedScanner;
        }

        public DTWAINHelper(
            PreviewScannedImages viewScanImages, ConfigHelper conf, Form1 form1,
            ListBox scannerBox, Label scannerLabel, 
            ComboBox sourceBox, ComboBox paperBox, ComboBox colorBox, 
            NumericUpDown resolutionInput, TrackBar brightnessInput, Label brightnessLabel, TrackBar contrastInput, Label contrastLabel,
            NumericUpDown maxAcqInput, NumericUpDown maxPageInput, CheckBox sourceUiCheck, CheckBox sourceIndicatorCheck,
            TrackBar qualityInput, Label qualityLabel,
            Button stopScanButton
        )
        {
            this.ViewScanImages = viewScanImages;
            this.Conf = conf;
            this.form1 = form1;

            this.scannerBox = scannerBox;
            this.scannerLabel = scannerLabel;
            this.sourceBox = sourceBox;
            this.paperBox = paperBox;
            this.colorBox = colorBox;
            this.resolutionInput = resolutionInput;
            this.brightnessInput = brightnessInput;
            this.brightnessLabel = brightnessLabel;
            this.contrastInput = contrastInput;
            this.contrastLabel = contrastLabel;
            this.maxAcqInput = maxAcqInput;
            this.maxPageInput = maxPageInput;
            this.sourceUICheckBox = sourceUiCheck;
            this.sourceIndicatorCheckBox = sourceIndicatorCheck;
            this.qualityInput = qualityInput;
            this.qualityLabel = qualityLabel;
            this.stopScanButton = stopScanButton;
            
            this.InitDoGetScanWorker();
            this.InitDoSetScanWorker();
            this.InitDoScanWorker();
        }

        public void ClearSelectedScanner(bool NeedClearSelectedScannerBox)
        {
            DTWAINHelper.SelectedScanner = "";
            if (NeedClearSelectedScannerBox)
                scannerBox.ClearSelected();
            scannerLabel.Text = "No selected scanner";
            sourceBox.Items.Clear();
            sourceBox.Items.Add("Flatbed");
            sourceBox.SelectedIndex = 0;
            paperBox.SelectedIndex = 0;
            paperBox.Enabled = false;
            colorBox.SelectedIndex = ConfigHelper.Conf.Color;
            resolutionInput.Value = ConfigHelper.Conf.Resolution;
            brightnessInput.Value = ConfigHelper.Conf.Brightness;
            brightnessLabel.Text = ConfigHelper.Conf.Brightness.ToString();
            contrastInput.Value = ConfigHelper.Conf.Contrast;
            contrastLabel.Text = ConfigHelper.Conf.Contrast.ToString();
            maxAcqInput.Value = ConfigHelper.Conf.MaxAcq;
            maxPageInput.Value = ConfigHelper.Conf.MaxPage;
            sourceUICheckBox.Checked = ConfigHelper.Conf.SourceUI;
            sourceIndicatorCheckBox.Checked = false;
            qualityInput.Value = ConfigHelper.Conf.Quality;
            qualityLabel.Text = ConfigHelper.Conf.Quality.ToString();
        }
        private void ClearSelectedScanner()
        {
            this.ClearSelectedScanner(true);
        }
        private void SetSelectedScanner(ScannerProperties scanner)
        {
            this.ClearSelectedScanner(false);
            int PickedScanner = -1;
            for (int IScanner = 0; PickedScanner < 0 && IScanner < scannerBox.Items.Count; IScanner++)
            {
                if (scannerBox.Items[IScanner].ToString() == scanner.ProductName)
                    PickedScanner = IScanner;
            }
            if (PickedScanner < 0) return;
            scannerBox.SelectedIndex = PickedScanner;
            DTWAINHelper.SelectedScanner = scanner.ProductName;
            scannerLabel.Text = scanner.ProductName;
            if (scanner.IsSupportFeeder)
            {
                paperBox.Enabled = true;
                sourceBox.Items.Add("Feeder (1 Side)");
                if (scanner.IsSupportDuplex)
                    sourceBox.Items.Add("Feeder (2 Side)");
                if (DTWAINHelper.SelectedScanner.Equals(ConfigHelper.Conf.Scanner))
                {
                    paperBox.SelectedIndex = ConfigHelper.Conf.Paper;
                    if (ConfigHelper.Conf.Source <= 1 || scanner.IsSupportDuplex)
                    {
                        sourceBox.SelectedIndex = ConfigHelper.Conf.Source;
                    }
                }
            }
            sourceUICheckBox.Checked = false;
            if (scanner.IsSupportIndicator)
            {
                sourceIndicatorCheckBox.Checked = true;
                sourceIndicatorCheckBox.Enabled = true;
            }
            else
            {
                sourceIndicatorCheckBox.Checked = false;
                sourceIndicatorCheckBox.Enabled = false;
            }
        }

        private void InitDoGetScanWorker()
        {
            DoGetScanWorker.WorkerReportsProgress = true;
            DoGetScanWorker.WorkerSupportsCancellation = false;

            DoGetScanWorker.DoWork += (object sender, DoWorkEventArgs e) =>
            {
                DoGetScanResult result = new DoGetScanResult();
                result.Scanners = new List<String>();
                result.SelectedScanner = null;

                if (DTWAINHelper.IsDTWAINBusy)
                {
                    DoGetScanWorker.ReportProgress(-1, "DTWAIN_BUSY");
                    return;
                }
                if (TwainAPI.DTWAIN_IsTwainAvailable() != 1)
                {
                    DoGetScanWorker.ReportProgress(-1, "DTWAIN_NOT_FOUND");
                    return;
                }
                Stopwatch S = new Stopwatch();
                int RetS = 0;
                DoGetScanWorker.ReportProgress(1, "DTWAIN_StartThread");
                S.Restart();
                RetS = 0;
                while (RetS == 0 && S.ElapsedMilliseconds < 5_000)
                    RetS = TwainAPI.DTWAIN_StartThread(DTWAINHelper.DTWAINThread);
                if (RetS == 0)
                {
                    DoGetScanWorker.ReportProgress(-1, "DTWAIN_StartThread_FAILED");
                    return;
                }
                DoGetScanWorker.ReportProgress(2, "DTWAIN_StartTwainSession");
                S.Restart();
                RetS = 0;
                while (RetS == 0 && S.ElapsedMilliseconds < 5_000)
                    //StartTwainSession first argument parent window, second argument DLL file;
                    RetS = TwainAPI.DTWAIN_StartTwainSession(IntPtr.Zero, null);
                if (RetS == 0)
                {
                    DoGetScanWorker.ReportProgress(-1, "DTWAIN_StartTwainSession_FAILED");
                    S.Restart();
                    RetS = 0;
                    while (RetS == 0 && S.ElapsedMilliseconds < 1_000)
                        RetS = TwainAPI.DTWAIN_EndThread(DTWAINHelper.DTWAINThread);
                    if (RetS == 0)
                        DoGetScanWorker.ReportProgress(-1, "DTWAIN_EndThread_FAILED");
                    return;
                }
                DTWAINHelper.IsDTWAINBusy = true;

                DTWAIN_ARRAY PtrArraySources = IntPtr.Zero;
                DoGetScanWorker.ReportProgress(3, "DTWAIN_EnumSources");
                TwainAPI.DTWAIN_EnumSources(ref PtrArraySources);
                if (PtrArraySources != IntPtr.Zero)
                {
                    DoGetScanWorker.ReportProgress(4, "DTWAIN_ArrayGetCount");
                    int CountSources = TwainAPI.DTWAIN_ArrayGetCount(PtrArraySources);
                    if (CountSources > 0)
                    {
                        int PercentageEachSource = 80 / CountSources;
                        for (int ISource = 0; ISource < CountSources; ISource++)
                        {
                            DoGetScanWorker.ReportProgress(10 + ISource * PercentageEachSource, "DTWAIN_ArrayGetSourceAt");
                            DTWAIN_SOURCE PtrSource = IntPtr.Zero;
                            TwainAPI.DTWAIN_ArrayGetSourceAt(PtrArraySources, ISource, ref PtrSource);
                            if (PtrSource != IntPtr.Zero)
                            {
                                StringBuilder SourceName = new StringBuilder(256);
                                TwainAPI.DTWAIN_GetSourceProductName(PtrSource, SourceName, 255);
                                result.Scanners.Add(SourceName.ToString());
                                if ((
                                (DTWAINHelper.SelectedScanner.Length == 0 && ConfigHelper.Conf.Scanner.Equals(SourceName.ToString())) ||
                                DTWAINHelper.SelectedScanner.Equals(SourceName.ToString())
                                ) && result.SelectedScanner == null)
                                {
                                    S.Restart();
                                    while (TwainAPI.DTWAIN_IsSourceOpen(PtrSource) == 0 && S.ElapsedMilliseconds < 5_000)
                                        TwainAPI.DTWAIN_OpenSource(PtrSource);
                                    if (TwainAPI.DTWAIN_IsSourceOpen(PtrSource) == 1)
                                    {
                                        result.SelectedScanner = new ScannerProperties();
                                        result.SelectedScanner.ProductName = SourceName.ToString();
                                        result.SelectedScanner.IsSupportFeeder = TwainAPI.DTWAIN_IsFeederSupported(PtrSource) == 1;
                                        if (result.SelectedScanner.IsSupportFeeder)
                                        {
                                            result.SelectedScanner.IsSupportDuplex = TwainAPI.DTWAIN_IsDuplexSupported(PtrSource) == 1;
                                        }
                                        else
                                        {
                                            result.SelectedScanner.IsSupportDuplex = false;
                                        }
                                        result.SelectedScanner.IsSupportIndicator = TwainAPI.DTWAIN_IsIndicatorSupported(PtrSource) == 1;
                                        S.Restart();
                                        while (TwainAPI.DTWAIN_IsSourceOpen(PtrSource) == 1 && S.ElapsedMilliseconds < 5_000)
                                            TwainAPI.DTWAIN_CloseSource(PtrSource);
                                        if (TwainAPI.DTWAIN_IsSourceOpen(PtrSource) == 1)
                                            DoGetScanWorker.ReportProgress(-1, "DTWAIN_CloseSource_FAILED");
                                    } 
                                    else
                                    {
                                        DoGetScanWorker.ReportProgress(-1, "SCANNER_NOT_OPENED");
                                    }
                                }
                            }
                            else
                            {
                                DoGetScanWorker.ReportProgress(-1, "SCANNER_NOT_FOUND");
                            }
                        }
                    }
                }
                else
                {
                    DoGetScanWorker.ReportProgress(-1, "SCANNERS_ENUM_NOT_FOUND");
                }

                DoGetScanWorker.ReportProgress(98, "DTWAIN_EndTwainSession");
                S.Restart();
                RetS = 0;
                while (RetS == 0 && S.ElapsedMilliseconds < 5_000)
                    RetS = TwainAPI.DTWAIN_EndTwainSession();
                if (RetS == 0)
                    DoGetScanWorker.ReportProgress(-1, "DTWAIN_EndTwainSession_FAILED");
                DoGetScanWorker.ReportProgress(99, "DTWAIN_EndThread");
                S.Restart();
                RetS = 0;
                while (RetS == 0 && S.ElapsedMilliseconds < 1_000)
                    RetS = TwainAPI.DTWAIN_EndThread(DTWAINHelper.DTWAINThread);
                /*if (RetS == 0)
                    DoGetScanWorker.ReportProgress(-1, "DTWAIN_EndThread_FAILED");*/
                DTWAINHelper.IsDTWAINBusy = false;

                e.Result = result;
            };

            DoGetScanWorker.ProgressChanged += (object sender, ProgressChangedEventArgs e) =>
            {
                String message = "";
                if (e.UserState is String)
                {
                    message = (String)e.UserState;
                }
                if (e.ProgressPercentage == -1)
                {
                    if (this.form1.WindowState == FormWindowState.Normal && this.form1.TopLevel == true)
                    {
                        MessageBox.Show(message, "ERROR");
                    }
                    DTWAINHelper.SaveLog("DoGetScanWorker", "onGetProggress : " + message);
                }
                if (e.ProgressPercentage > 0)
                {
                    this.scannerLabel.Text = "(" + e.ProgressPercentage.ToString() + "% " + message + ")";
                    Debug.WriteLine("(" + e.ProgressPercentage.ToString() + "% " + message + ")");
                    DTWAINHelper.SaveLog("DoGetScanWorker", "onGetProggress : " + "(" + e.ProgressPercentage.ToString() + "% " + message + ")");
                }
            };
            DoGetScanWorker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) =>
            {
                DoGetScanResult result;
                if (e.Result is DoGetScanResult)
                    result = (DoGetScanResult)e.Result;
                else
                    return;
                scannerBox.Items.Clear();
                scannerBox.Items.AddRange(result.Scanners.ToArray());
                if (result.SelectedScanner == null)
                {
                    this.ClearSelectedScanner();
                }
                else
                {
                    this.SetSelectedScanner(result.SelectedScanner);
                }
            };
        }

        public void DoGetScannerList()
        {
            if (!DoGetScanWorker.IsBusy)
            {
                DoGetScanWorker.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Get Scan Worker Busy", "ERROR");
            }
        }

        private void InitDoSetScanWorker()
        {
            DoSetScanWorker.WorkerReportsProgress = true;
            DoSetScanWorker.WorkerSupportsCancellation = false;

            DoSetScanWorker.DoWork += (object sender, DoWorkEventArgs e) =>
            {
                string args = "";
                ScannerProperties? result = null;
                if (e.Argument is String)
                {
                    args = (String)e.Argument;
                }
                else
                {
                    DoSetScanWorker.ReportProgress(-1, "ARGS_INVALID");
                    return;
                }
                if (DTWAINHelper.IsDTWAINBusy)
                {
                    DoSetScanWorker.ReportProgress(-1, "DTWAIN_BUSY");
                    return;
                }
                if (TwainAPI.DTWAIN_IsTwainAvailable() != 1)
                {
                    DoSetScanWorker.ReportProgress(-1, "DTWAIN_NOT_FOUND");
                    return;
                }
                Stopwatch S = new Stopwatch();
                int RetS = 0;
                DoSetScanWorker.ReportProgress(1, "DTWAIN_StartThread");
                S.Restart();
                RetS = 0;
                while (RetS == 0 && S.ElapsedMilliseconds < 5_000)
                    RetS = TwainAPI.DTWAIN_StartThread(DTWAINHelper.DTWAINThread);
                if (RetS == 0)
                {
                    DoSetScanWorker.ReportProgress(-1, "DTWAIN_StartThread_FAILED");
                    return;
                }
                DoSetScanWorker.ReportProgress(2, "DTWAIN_StartTwainSession");
                S.Restart();
                RetS = 0;
                while (RetS == 0 && S.ElapsedMilliseconds < 5_000)
                    //StartTwainSession first argument parent window, second argument DLL file;
                    RetS = TwainAPI.DTWAIN_StartTwainSession(IntPtr.Zero, null);
                if (RetS == 0)
                {
                    DoSetScanWorker.ReportProgress(-1, "DTWAIN_StartTwainSession_FAILED");
                    S.Restart();
                    RetS = 0;
                    while (RetS == 0 && S.ElapsedMilliseconds < 1_000)
                        RetS = TwainAPI.DTWAIN_EndThread(DTWAINHelper.DTWAINThread);
                    if (RetS == 0)
                        DoSetScanWorker.ReportProgress(-1, "DTWAIN_EndThread_FAILED");
                    return;
                }
                DTWAINHelper.IsDTWAINBusy = true;

                DoSetScanWorker.ReportProgress(3, "DTWAIN_SelectSourceByName");
                DTWAIN_SOURCE PtrSouce = TwainAPI.DTWAIN_SelectSourceByName(args);

                if (PtrSouce != IntPtr.Zero)
                {
                    DoSetScanWorker.ReportProgress(4, "DTWAIN_OpenSource");
                    S.Restart();
                    while (TwainAPI.DTWAIN_IsSourceOpen(PtrSouce) == 0 && S.ElapsedMilliseconds < 5_000)
                        TwainAPI.DTWAIN_OpenSource(PtrSouce);
                    if (TwainAPI.DTWAIN_IsSourceOpen(PtrSouce) == 1)
                    {
                        //Source Setting
                        DoSetScanWorker.ReportProgress(5, "DTWAIN_IsFeederDuplexSupported");
                        result = new ScannerProperties();
                        result.ProductName = args;
                        result.IsSupportFeeder = TwainAPI.DTWAIN_IsFeederSupported(PtrSouce) == 1;
                        if (result.IsSupportFeeder)
                        {
                            result.IsSupportDuplex = TwainAPI.DTWAIN_IsDuplexSupported(PtrSouce) == 1;
                        }
                        else
                        {
                            result.IsSupportDuplex = false;
                        }
                        DoSetScanWorker.ReportProgress(6, "DTWAIN_IsFeederDuplexSupported");
                        result.IsSupportIndicator = TwainAPI.DTWAIN_IsIndicatorSupported(PtrSouce) == 1;
                        DoSetScanWorker.ReportProgress(97, "DTWAIN_CloseSource");
                        S.Restart();
                        while (TwainAPI.DTWAIN_IsSourceOpen(PtrSouce) == 1 && S.ElapsedMilliseconds < 5_000)
                            TwainAPI.DTWAIN_CloseSource(PtrSouce);
                        if (TwainAPI.DTWAIN_IsSourceOpen(PtrSouce) == 1)
                            DoSetScanWorker.ReportProgress(-1, "DTWAIN_CloseSource_FAILED");
                    }
                    else
                    {
                        DoSetScanWorker.ReportProgress(-1, "SCANNER_NOT_OPENED");
                    }
                }
                else
                {
                    DoSetScanWorker.ReportProgress(-1, "SCANNER_NOT_FOUND");
                }

                DoSetScanWorker.ReportProgress(98, "DTWAIN_EndTwainSession");
                S.Restart();
                RetS = 0;
                while (RetS == 0 && S.ElapsedMilliseconds < 5_000)
                    RetS = TwainAPI.DTWAIN_EndTwainSession();
                if (RetS == 0)
                    DoSetScanWorker.ReportProgress(-1, "DTWAIN_EndTwainSession_FAILED");
                DoSetScanWorker.ReportProgress(99, "DTWAIN_EndThread");
                S.Restart();
                RetS = 0;
                while (RetS == 0 && S.ElapsedMilliseconds < 1_000)
                    RetS = TwainAPI.DTWAIN_EndThread(DTWAINHelper.DTWAINThread);
                /*if (RetS == 0)
                    DoSetScanWorker.ReportProgress(-1, "DTWAIN_EndThread_FAILED");*/

                DTWAINHelper.IsDTWAINBusy = false;

                e.Result = result;
            };
            DoSetScanWorker.ProgressChanged += (object sender, ProgressChangedEventArgs e) =>
            {
                String message = "";
                if (e.UserState is String)
                {
                    message = (String)e.UserState;
                }
                if (e.ProgressPercentage == -1)
                {
                    if (this.form1.WindowState == FormWindowState.Normal && this.form1.TopLevel == true)
                    {
                        MessageBox.Show(message, "ERROR");
                    }
                    DTWAINHelper.SaveLog("DoSetScanWorker", "onSetError : "+message);
                }
                if (e.ProgressPercentage > 0)
                {
                    this.scannerLabel.Text = "(" + e.ProgressPercentage.ToString() + "% " + message + ")";
                    Debug.WriteLine("(" + e.ProgressPercentage.ToString() + "% " + message + ")");
                    DTWAINHelper.SaveLog("DoSetScanWorker", "onSetProggress : " + "(" + e.ProgressPercentage.ToString() + "% " + message + ")");
                }
            };
            DoSetScanWorker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) =>
            {
                if (e.Result is ScannerProperties)
                {
                    this.SetSelectedScanner((ScannerProperties)e.Result);
                }
                else
                {
                    this.ClearSelectedScanner();

                }
            };
        }

        public void SetSelectedScanner()
        {
            if (this.scannerBox.SelectedIndex < 0 || !(this.scannerBox.SelectedItem is String))
            {
                this.ClearSelectedScanner(false);
                return;
            }
            if (!DoSetScanWorker.IsBusy)
            {
                DoSetScanWorker.RunWorkerAsync(this.scannerBox.SelectedItem);
            }
            else
            {
                MessageBox.Show("Set Scan Worker Busy", "ERROR");
            }
        }

        private void InitDoScanWorker()
        {
            DoScanWorker.WorkerReportsProgress = true;
            DoScanWorker.WorkerSupportsCancellation = false;

            DoScanWorker.DoWork += (object sender, DoWorkEventArgs e) =>
            {
                this.ViewScanImages.ClearImages();
                JsonObject result = new JsonObject();
                result["request"] = "onScanDone";
                DoScanArguments args = new DoScanArguments();
                if (e.Argument is DoScanArguments)
                {
                    args = (DoScanArguments)e.Argument;
                }
                else
                {
                    DoScanWorker.ReportProgress(-1, "ARGS_INVALID");
                    return;
                }
                String CurrentSelectedScanner = DTWAINHelper.SelectedScanner;
                if (DTWAINHelper.SelectedScanner.Length == 0)
                {
                    args.Source = ConfigHelper.Conf.Source;
                    args.Paper = ConfigHelper.Conf.Paper;
                    CurrentSelectedScanner = ConfigHelper.Conf.Scanner;
                }
                if (CurrentSelectedScanner.Length == 0)
                {
                    DoScanWorker.ReportProgress(-1, "SCANNER_NULL");
                    return;
                }
                if (DTWAINHelper.IsDTWAINBusy)
                {
                    DoScanWorker.ReportProgress(-1, "DTWAIN_BUSY");
                    return;
                }
                if (TwainAPI.DTWAIN_IsTwainAvailable() != 1)
                {
                    DoScanWorker.ReportProgress(-1, "DTWAIN_NOT_FOUND");
                    return;
                }

                //START CONSTRUCT ENCODER
                ImageCodecInfo JpegCodecInfo = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                EncoderParameters EncoderParams = new EncoderParameters();

                //Param for image quality
                System.Drawing.Imaging.Encoder EncoderTypeQuality = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameter EncodeParamQuality = new EncoderParameter(EncoderTypeQuality, args.Quality);
                EncoderParams.Param[0] = EncodeParamQuality;

                //END CONSTRUCT ENCODER
                DTWAIN_HANDLE ThreadSession = IntPtr.Zero;
                if(this.formStatus != null)
                {
                    this.formStatus.Dispose();
                }
                if (args.SourceIndicator || args.SourceIndicatorEnable != true)
                {
                    this.formStatus = new Form2();
                    this.formStatus.Show();
                    this.formStatus.Invoke((MethodInvoker)delegate
                    {
                        this.formStatus.setStatus();
                        this.formStatus.TopLevel = true;
                        this.formStatus.TopMost = true;
                    });
                    ThreadSession = this.formStatus.Handle;
                }
                else
                {
                    this.formStatus = null;
                }
                DTWAINHelper.SaveLog("DoScanWorker", "Start DoScanWorker");
                Stopwatch S = new Stopwatch();
                int RetS = 0;
                DoScanReportProgressInterceptor(1, "DTWAIN_StartThread");
                S.Restart();
                RetS = 0;
                while (RetS == 0 && S.ElapsedMilliseconds < 5_000)
                    RetS = TwainAPI.DTWAIN_StartThread(DTWAINHelper.DTWAINThread);
                if (RetS == 0)
                {
                    DoScanReportProgressInterceptor(-1, "DTWAIN_StartThread_FAILED");
                    if (this.formStatus != null)
                        this.formStatus.Dispose();
                    this.formStatus = null;
                    return;
                }
                DoScanReportProgressInterceptor(2, "DTWAIN_StartTwainSession");
                S.Restart();
                RetS = 0;
                while (RetS == 0 && S.ElapsedMilliseconds < 5_000)
                    //StartTwainSession first argument parent window, second argument DLL file;
                    RetS = TwainAPI.DTWAIN_StartTwainSession(ThreadSession, null);
                if (RetS == 0)
                {
                    DoScanReportProgressInterceptor(-1, "DTWAIN_StartTwainSession_FAILED");
                    S.Restart();
                    RetS = 0;
                    while (RetS == 0 && S.ElapsedMilliseconds < 1_000)
                        RetS = TwainAPI.DTWAIN_EndThread(DTWAINHelper.DTWAINThread);
                    if (RetS == 0)
                        DoScanReportProgressInterceptor(-1, "DTWAIN_EndThread_FAILED");
                    if (this.formStatus != null)
                        this.formStatus.Dispose();
                    this.formStatus = null;
                    return;
                }
                DTWAINHelper.IsDTWAINBusy = true;
                DTWAINHelper.KeepScanning = true;
                DoScanReportProgressInterceptor(3, "DTWAIN_SetCallback");
                //Reason Comment : Notify make application unstable during acquire
                //TwainAPI.DTWAIN_EnableMsgNotify(1);
                //TwainAPI.DTWAIN_SetCallback(this.AcquireCallback, 0);

                DoScanReportProgressInterceptor(4, "DTWAIN_SelectSourceByName");
                DTWAIN_SOURCE PtrSouce = TwainAPI.DTWAIN_SelectSourceByName(CurrentSelectedScanner);

                if (PtrSouce != IntPtr.Zero)
                {
                    DoScanReportProgressInterceptor(5, "DTWAIN_OpenSource");
                    S.Restart();
                    while(TwainAPI.DTWAIN_IsSourceOpen(PtrSouce) == 0 && S.ElapsedMilliseconds < 5_000)
                        TwainAPI.DTWAIN_OpenSource(PtrSouce);
                    if (TwainAPI.DTWAIN_IsSourceOpen(PtrSouce) == 1)
                    {
                        //Source Setting
                        DoScanWorker.ReportProgress(5, "DTWAIN_IsIndicatorSupported");
                        if (TwainAPI.DTWAIN_IsIndicatorSupported(PtrSouce) == 1)
                        {
                            DoScanWorker.ReportProgress(6, "DTWAIN_EnableIndicator");
                            TwainAPI.DTWAIN_EnableIndicator(PtrSouce, args.SourceIndicator ? 1 : 0);
                        }
                        DoScanReportProgressInterceptor(7, "DTWAIN_IsFeederDuplexSupported");
                        if (TwainAPI.DTWAIN_IsFeederSupported(PtrSouce) == 1)
                        {
                            DoScanReportProgressInterceptor(8, "DTWAIN_IsFeederLoaded");
                            if (args.Source == 0 || TwainAPI.DTWAIN_IsFeederLoaded(PtrSouce) == 0)
                            {
                                DoScanReportProgressInterceptor(9, "DTWAIN_EnableFeeder0");
                                TwainAPI.DTWAIN_EnableFeeder(PtrSouce, 0);
                            }
                            else
                            {
                                DoScanReportProgressInterceptor(9, "DTWAIN_EnableFeeder1");
                                TwainAPI.DTWAIN_EnableFeeder(PtrSouce, 1);
                                if (TwainAPI.DTWAIN_IsDuplexSupported(PtrSouce) == 1)
                                {
                                    if (args.Source == 2)
                                    {
                                        DoScanReportProgressInterceptor(10, "DTWAIN_EnableDuplex1");
                                        TwainAPI.DTWAIN_EnableDuplex(PtrSouce, 1);
                                    }
                                    else
                                    {
                                        DoScanReportProgressInterceptor(10, "DTWAIN_EnableDuplex0");
                                        TwainAPI.DTWAIN_EnableDuplex(PtrSouce, 0);
                                    }
                                }
                            }
                        }
                        //Max Acq
                        DoScanReportProgressInterceptor(11, "DTWAIN_SetPaperSize");
                        TwainAPI.DTWAIN_SetMaxAcquisitions(PtrSouce, args.MaxAcq > 0 ? args.MaxAcq : TwainAPI.DTWAIN_MAXACQUIRE);
                        //Paper
                        DoScanReportProgressInterceptor(12, "DTWAIN_SetPaperSize");
                        TwainAPI.DTWAIN_SetPaperSize(PtrSouce, args.Paper, args.Paper == 0 ? 0 : 1);
                        //Resolution
                        DoScanReportProgressInterceptor(13, "DTWAIN_SetResolution");
                        TwainAPI.DTWAIN_SetResolution(PtrSouce, args.Resolution);
                        //Brightness
                        DoScanReportProgressInterceptor(14, "DTWAIN_SetBrightness");
                        TwainAPI.DTWAIN_SetBrightness(PtrSouce, args.Brightness);
                        //Contrast
                        DoScanReportProgressInterceptor(15, "DTWAIN_SetContrast");
                        TwainAPI.DTWAIN_SetContrast(PtrSouce, args.Contrast);

                        //Acquisition Array
                        //Inside Acquistion Array are DIB Array
                        DoScanReportProgressInterceptor(16, "DTWAIN_CreateAcquisitionArray");
                        DTWAIN_ARRAY AcqArray = TwainAPI.DTWAIN_CreateAcquisitionArray();
                        try
                        {
                            int pStatus = 0;
                            DoScanReportProgressInterceptor(17, "DTWAIN_AcquireNativeEx");
                            int AcquireCodeResult = 0;
                            DTWAINHelper.SaveLog("DoScanWorker", "Start Acquisition");
                            AcquireCodeResult = TwainAPI.DTWAIN_AcquireNativeEx(
                                PtrSouce,
                                args.Color,
                                args.MaxPage == 0 ? TwainAPI.DTWAIN_ACQUIREALL : args.MaxPage,
                                args.SourceUI ? 1 : 0,
                                0,
                                AcqArray,
                                ref pStatus
                            );
                            DTWAINHelper.SaveLog("DoScanWorker", "Finish Acquisition");
                            if (AcquireCodeResult == 1)
                            {
                                // 20% - 80%
                                int CountAcq = TwainAPI.DTWAIN_GetNumAcquisitions(AcqArray);
                                int PercentEachAcq = 60 / CountAcq;
                                JsonArray JAcq = new JsonArray();
                                for (int IAcq = 0; IAcq < CountAcq; IAcq++)
                                {
                                    DoScanReportProgressInterceptor(20 + IAcq * PercentEachAcq, "PROCESS_ACQ");
                                    int CountDib = TwainAPI.DTWAIN_GetNumAcquiredImages(AcqArray, IAcq);
                                    int PercentEachDib = PercentEachAcq / CountDib;
                                    JsonArray JDib = new JsonArray();
                                    for (int IDib = 0; IDib < CountDib; IDib++)
                                    {
                                        DoScanReportProgressInterceptor(20 + IAcq * PercentEachAcq + IDib * PercentEachDib, "PROCESS_DIB");
                                        DTWAIN_HANDLE PtrDib = TwainAPI.DTWAIN_GetAcquiredImage(AcqArray, IAcq, IDib);
                                        Bitmap BmpScan = Bitmap.FromHbitmap(TwainAPI.DTWAIN_ConvertDIBToBitmap(PtrDib, IntPtr.Zero));
                                        MemoryStream StreamScan = new MemoryStream();
                                        BmpScan.Save(StreamScan, JpegCodecInfo, EncoderParams);
                                        String Base64Scan = Convert.ToBase64String(StreamScan.ToArray());
                                        this.ViewScanImages.AddImage(IAcq, IDib, StreamScan);
                                        JDib.Add("data:image/jpeg;base64," + Base64Scan);
                                        BmpScan.Dispose();
                                        /*Debug.WriteLine(Base64Scan);*/
                                    }
                                    JAcq.Add(JDib);
                                }
                                result["data"] = JAcq;
                                DTWAINHelper.SaveLog("DoScanWorker", "Finish Process Acquisition");
                            }
                            else
                            {
                                DoScanReportProgressInterceptor(-1, "ACQ_FAILED");
                            }
                        }
                        catch (Exception ex)
                        {
                            DoScanReportProgressInterceptor(-1, "ACQ_FAILED");
                            DoScanReportProgressInterceptor(-1, ex.Message);
                        }
                        DoScanReportProgressInterceptor(96, "DTWAIN_DestroyAcquisitionArray");
                        S.Restart();
                        RetS = 0;
                        while (RetS == 0 && S.ElapsedMilliseconds < 300_000)
                            RetS = TwainAPI.DTWAIN_DestroyAcquisitionArray(AcqArray, 1);
                        if(RetS == 0)
                            DoScanReportProgressInterceptor(-1, "DTWAIN_DestroyAcquisitionArray_FAILED");
                        DoScanReportProgressInterceptor(97, "DTWAIN_CloseSource");
                        S.Restart();
                        while (TwainAPI.DTWAIN_IsSourceOpen(PtrSouce) == 1 && S.ElapsedMilliseconds < 5_000)
                            TwainAPI.DTWAIN_CloseSource(PtrSouce);
                        if (TwainAPI.DTWAIN_IsSourceOpen(PtrSouce) == 1)
                            DoScanReportProgressInterceptor(-1, "DTWAIN_CloseSource_FAILED");
                    }
                    else
                    {
                        DoScanReportProgressInterceptor(-1, "SCANNER_NOT_OPENED");
                    }
                }
                else
                {
                    DoScanReportProgressInterceptor(-1, "SCANNER_NOT_FOUND");
                }

                DoScanReportProgressInterceptor(98, "DTWAIN_EndTwainSession");
                S.Restart();
                RetS = 0;
                while (RetS == 0 && S.ElapsedMilliseconds < 5_000)
                    RetS = TwainAPI.DTWAIN_EndTwainSession();
                if (RetS == 0)
                    DoScanReportProgressInterceptor(-1, "DTWAIN_EndTwainSession_FAILED");
                DoScanReportProgressInterceptor(99, "DTWAIN_EndThread");
                S.Restart();
                RetS = 0;
                while (RetS == 0 && S.ElapsedMilliseconds < 1_000)
                    RetS = TwainAPI.DTWAIN_EndThread(DTWAINHelper.DTWAINThread);
                /*if (RetS == 0)
                    DoScanReportProgressInterceptor(-1, "DTWAIN_EndThread_FAILED");*/
                DTWAINHelper.IsDTWAINBusy = false;
                DTWAINHelper.KeepScanning = false;
                DTWAINHelper.SaveLog("DoScanWorker", "Finish DoScanWorker");
                if (this.formStatus != null)
                    this.formStatus.Dispose();
                this.formStatus = null;
                e.Result = result.ToJsonString();
            };
            DoScanWorker.ProgressChanged += (object sender, ProgressChangedEventArgs e) =>
            {
                JsonObject result = new JsonObject();
                String code = "";
                if (e.UserState is String)
                {
                    code = (String)e.UserState;
                }
                if (e.ProgressPercentage == -1)
                {
                    result["request"] = "onScanError";
                    result["error"] = code;
                    DTWAINHelper.SaveLog("DoScanWorker", "onScanError : " + code);
                }
                if (e.ProgressPercentage == 0)
                {
                    Debug.WriteLine("Acquire Callback : " + code);
                    result["request"] = "onScanCallback";
                    result["status"] = code;
                    DTWAINHelper.SaveLog("DoScanWorker", "onScanCallback : " + code);
                }
                if (e.ProgressPercentage > 0)
                {
                    this.form1.Invoke((MethodInvoker)delegate
                    {
                        this.scannerLabel.Text = DTWAINHelper.SelectedScanner + " (" + e.ProgressPercentage.ToString() + "% " + code + ")";
                    });
                    DTWAINHelper.SaveLog("DoScanWorker", "onScanNotification : " + DTWAINHelper.SelectedScanner + " (" + e.ProgressPercentage.ToString() + "% " + code + ")");
                    Debug.WriteLine(DTWAINHelper.SelectedScanner + " (" + e.ProgressPercentage.ToString() + "% " + code + ")");
                    result["request"] = "onScanNotification";
                    result["percentage"] = e.ProgressPercentage;
                    result["status"] = code;
                }
                if (socket != null)
                {
                    if (socket.IsAvailable)
                        socket.Send(result.ToJsonString());
                    else
                        socket = null;
                }
                //show messagebox after send socket
                if (e.ProgressPercentage == -1)
                {
                    this.form1.Invoke((MethodInvoker)delegate
                    {
                        if (this.form1.WindowState == FormWindowState.Normal && this.form1.TopLevel == true)
                        {
                            MessageBox.Show(code, "ERROR");
                        }
                    });
                }
            };
            DoScanWorker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) =>
            {
                this.form1.Invoke((MethodInvoker)delegate
                {
                    this.stopScanButton.Enabled = false;
                    if (DTWAINHelper.SelectedScanner.Length > 0)
                    {
                        this.scannerLabel.Text = DTWAINHelper.SelectedScanner;
                    }
                    else
                    {
                        this.ClearSelectedScanner();
                    }
                });
                if (socket != null)
                {
                    DTWAINHelper.SaveLog("DoScanWorker", "Start Send to WebSocket");
                    if (e.Result is string && socket.IsAvailable)
                        socket.Send((string)e.Result);
                    socket = null;
                    DTWAINHelper.SaveLog("DoScanWorker", "Finish Send to WebSocket");
                }
                else
                {
                    DTWAINHelper.SaveLog("DoScanWorker", "No Message to WebSocket");
                }
            };
        }
        private void DoScanReportProgressInterceptor(int ProgressPercentage, String code)
        {
            if (DoScanWorker.IsBusy)
            {
                DoScanWorker.ReportProgress(ProgressPercentage, code);
            }
            if (ProgressPercentage > 0 && this.formStatus != null && this.formStatus.IsHandleCreated && this.formStatus.Visible)
            {
                this.formStatus.Invoke((MethodInvoker)delegate
                {
                    this.formStatus.setStatus(DTWAINHelper.SelectedScanner + " (" + ProgressPercentage.ToString() + "% " + code + ")");
                });
            }
        }

        private DoScanArguments GetScanArguments()
        {
            DoScanArguments args = new DoScanArguments();
            this.form1.Invoke((MethodInvoker)delegate
            {
                args.Source = this.sourceBox.SelectedIndex;
                args.Paper = PaperInts[this.paperBox.SelectedIndex];
                args.Color = ColorInts[this.colorBox.SelectedIndex];
                args.Resolution = (int)this.resolutionInput.Value;
                args.Brightness = this.brightnessInput.Value;
                args.Contrast = this.contrastInput.Value;
                args.MaxAcq = (int)this.maxAcqInput.Value;
                args.MaxPage = (int)this.maxPageInput.Value;
                args.SourceUI = this.sourceUICheckBox.Checked;
                args.SourceIndicator = this.sourceIndicatorCheckBox.Checked;
                args.SourceIndicatorEnable = this.sourceIndicatorCheckBox.Enabled;
                args.Quality = this.qualityInput.Value;
            });
            return args;
        }

        public void DoScan()
        {
            if (!DoScanWorker.IsBusy)
            {
                this.stopScanButton.Enabled = true;
                DoScanWorker.RunWorkerAsync(this.GetScanArguments());
            }
            else
            {
                MessageBox.Show("Scan Worker Busy", "ERROR");
            }
        }

        public void DoScanSocket(IWebSocketConnection socket)
        {
            JsonObject result = new JsonObject();
            if (DoScanWorker.IsBusy || DTWAINHelper.socket != null)
            {
                result["request"] = "onScanError";
                result["error"] = "WORKER_SCAN_BUSY";
                socket.Send(result.ToJsonString());
                return;
            }
            DTWAINHelper.socket = socket;
            if (!DoScanWorker.IsBusy)
            {
                this.form1.Invoke((MethodInvoker)delegate
                {
                    this.stopScanButton.Enabled = true;
                });
                DoScanWorker.RunWorkerAsync(this.GetScanArguments());
            }
            result["request"] = "onScanNotification";
            result["percentage"] = 0;
            result["status"] = "OK";
            socket.Send(result.ToJsonString());
        }

        private int AcquireCallback(int wParam, int lParam, int UserData)
        {
            String StrCode = "";
            int result = 1;
            switch (wParam)
            {
                case TwainAPI.DTWAIN_TN_ACQUIRESTARTED:
                    StrCode = "DTWAIN_TN_ACQUIRESTARTED";
                    break;
                case TwainAPI.DTWAIN_TN_ACQUIREDONE:
                    StrCode = "DTWAIN_TN_ACQUIREDONE";
                    break;
                case TwainAPI.DTWAIN_TN_ACQUIRETERMINATED:
                    StrCode = "DTWAIN_TN_ACQUIRETERMINATED";
                    break;
                case TwainAPI.DTWAIN_TN_ACQUIREFAILED:
                    StrCode = "DTWAIN_TN_ACQUIREFAILED";
                    break;
                case TwainAPI.DTWAIN_TN_ACQUIRECANCELLED:
                    StrCode = "DTWAIN_TN_ACQUIRECANCELLED";
                    break;
                /*case TwainAPI.DTWAIN_TN_ACQUIREPAGEDONE:
                    StrCode="DTWAIN_TN_ACQUIREPAGEDONE";
                    break;*/
                case TwainAPI.DTWAIN_TN_BLANKPAGEDETECTED1:
                    StrCode = "DTWAIN_TN_BLANKPAGEDETECTED1";
                    break;
                case TwainAPI.DTWAIN_TN_BLANKPAGEDETECTED2:
                    StrCode = "DTWAIN_TN_BLANKPAGEDETECTED2";
                    break;
                case TwainAPI.DTWAIN_TN_BLANKPAGEDETECTED3:
                    StrCode = "DTWAIN_TN_BLANKPAGEDETECTED3";
                    break;
                case TwainAPI.DTWAIN_TN_BLANKPAGEDISCARDED1:
                    StrCode = "DTWAIN_TN_BLANKPAGEDISCARDED1";
                    break;
                case TwainAPI.DTWAIN_TN_BLANKPAGEDISCARDED2:
                    StrCode = "DTWAIN_TN_BLANKPAGEDISCARDED2";
                    break;
                case TwainAPI.DTWAIN_TN_EOJDETECTED:
                    StrCode = "DTWAIN_TN_EOJDETECTED";
                    break;
                case TwainAPI.DTWAIN_TN_EOJDETECTED_XFERDONE:
                    StrCode = "DTWAIN_TN_EOJDETECTED_XFERDONE";
                    break;
                case TwainAPI.DTWAIN_TN_FILENAMECHANGING:
                    StrCode = "DTWAIN_TN_FILENAMECHANGING";
                    break;
                case TwainAPI.DTWAIN_TN_FILEPAGESAVEERROR:
                    StrCode = "DTWAIN_TN_FILEPAGESAVEERROR";
                    break;
                case TwainAPI.DTWAIN_TN_FILEPAGESAVING:
                    StrCode = "DTWAIN_TN_FILEPAGESAVING";
                    break;
                case TwainAPI.DTWAIN_TN_FILEPAGESAVEOK:
                    StrCode = "DTWAIN_TN_FILEPAGESAVEOK";
                    break;
                case TwainAPI.DTWAIN_TN_FILESAVECANCELLED:
                    StrCode = "DTWAIN_TN_FILESAVECANCELLED";
                    break;
                case TwainAPI.DTWAIN_ERR_TS_FILESAVEERROR:
                    StrCode = "DTWAIN_ERR_TS_FILESAVEERROR";
                    break;
                case TwainAPI.DTWAIN_TN_FILESAVEOK:
                    StrCode = "DTWAIN_TN_FILESAVEOK";
                    break;
                case TwainAPI.DTWAIN_TN_INVALIDIMAGEFORMAT:
                    StrCode = "DTWAIN_TN_INVALIDIMAGEFORMAT";
                    break;
                case TwainAPI.DTWAIN_TN_PAGEFAILED:
                    StrCode = "DTWAIN_TN_PAGEFAILED";
                    break;
                case TwainAPI.DTWAIN_TN_PAGECANCELLED:
                    StrCode = "DTWAIN_TN_PAGECANCELLED";
                    break;
                case TwainAPI.DTWAIN_TN_PAGECONTINUE:
                    StrCode = "DTWAIN_TN_PAGECONTINUE";
                    result = KeepScanning ? 1 : 0;
                    break;
                case TwainAPI.DTWAIN_TN_PAGEDISCARDED:
                    StrCode = "DTWAIN_TN_PAGEDISCARDED";
                    break;
                case TwainAPI.DTWAIN_TN_PROCESSEDDIB:
                    StrCode = "DTWAIN_TN_PROCESSEDDIB";
                    break;
                case TwainAPI.DTWAIN_TN_PROCESSEDDIBFINAL:
                    StrCode = "DTWAIN_TN_PROCESSEDDIBFINAL";
                    break;
                case TwainAPI.DTWAIN_TN_QUERYPAGEDISCARD:
                    StrCode = "DTWAIN_TN_QUERYPAGEDISCARD";
                    break;
                case TwainAPI.DTWAIN_TN_TWAINPAGECANCELLED:
                    StrCode = "DTWAIN_TN_TWAINPAGECANCELLED";
                    break;
                case TwainAPI.DTWAIN_TN_TWAINPAGEFAILED:
                    StrCode = "DTWAIN_TN_TWAINPAGEFAILED";
                    break;
                case TwainAPI.DTWAIN_TN_UIOPENED:
                    StrCode = "DTWAIN_TN_UIOPENED";
                    break;
                case TwainAPI.DTWAIN_TN_UICLOSING:
                    StrCode = "DTWAIN_TN_UICLOSING";
                    break;
                case TwainAPI.DTWAIN_TN_UICLOSED:
                    StrCode = "DTWAIN_TN_UICLOSED";
                    break;
                //For experienced TWAIN programmers
                case TwainAPI.DTWAIN_TN_TRANSFERREADY:
                    StrCode = "DTWAIN_TN_TRANSFERREADY";
                    break;
                case TwainAPI.DTWAIN_TN_TRANSFERDONE:
                    StrCode = "DTWAIN_TN_TRANSFERDONE";
                    break;
                case TwainAPI.DTWAIN_TN_TRANSFERCANCELLED:
                    StrCode = "DTWAIN_TN_TRANSFERCANCELLED";
                    break;
                case TwainAPI.DTWAIN_TN_TRANSFERSTRIPREADY:
                    StrCode = "DTWAIN_TN_TRANSFERSTRIPREADY";
                    break;
                case TwainAPI.DTWAIN_TN_TRANSFERSTRIPDONE:
                    StrCode = "DTWAIN_TN_TRANSFERSTRIPDONE";
                    break;
                case TwainAPI.DTWAIN_TN_TRANSFERSTRIPFAILED:
                    StrCode = "DTWAIN_TN_TRANSFERSTRIPFAILED";
                    break;
                //manual duplex
                case TwainAPI.DTWAIN_TN_MANDUPSIDE1START:
                    StrCode = "DTWAIN_TN_MANDUPSIDE1START:";
                    break;
                case TwainAPI.DTWAIN_TN_MANDUPSIDE1DONE:
                    StrCode = "DTWAIN_TN_MANDUPSIDE1DONE";
                    break;
                case TwainAPI.DTWAIN_TN_MANDUPSIDE2START:
                    StrCode = "DTWAIN_TN_MANDUPSIDE2START";
                    break;
                case TwainAPI.DTWAIN_TN_MANDUPSIDE2DONE:
                    StrCode = "DTWAIN_TN_MANDUPSIDE2DONE";
                    break;
                default:
                    StrCode = wParam.ToString();
                    break;

            }
            if (DoScanWorker.IsBusy)
            {
                DoScanWorker.ReportProgress(0, StrCode);
            }
            return result;
        }

        public void Cancel()
        {

        }

        public static void SaveLog(String Parent, String Message)
        {
            if (DTWAINHelper.SaveAppLogFile.Length > 0)
            {
                Process P = Process.GetCurrentProcess();
                long RAMConsumption = P.WorkingSet64;
                String RAMUsage = RAMConsumption.ToString() + " B";
                if (RAMConsumption > 1024)
                {
                    RAMConsumption /= 1024;
                    RAMUsage = RAMConsumption.ToString() + " KB";
                }
                if (RAMConsumption > 1024)
                {
                    RAMConsumption /= 1024;
                    RAMUsage = RAMConsumption.ToString() + " MB";
                }
                if (RAMConsumption > 1024)
                {
                    RAMConsumption /= 1024;
                    RAMUsage = RAMConsumption.ToString() + " GB";
                }
                String SaveToLog = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "UTC - " + Parent + " - RAM : " + RAMUsage + "\n" + Message + "\n";
                File.AppendAllText(DTWAINHelper.SaveAppLogFile, SaveToLog);
            }
        }
    }
}
