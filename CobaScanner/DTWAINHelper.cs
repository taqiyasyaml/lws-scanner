using System;
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

        public static bool StartDTWAIN()
        {
            if (TwainAPI.DTWAIN_IsTwainAvailable() != 1)
            {
                MessageBox.Show("DTWAIN_NOT_FOUND", "ERROR!");
                return false;
            }
            TwainAPI.DTWAIN_UseMultipleThreads(1);
            DTWAINHelper.DTWAINThread = TwainAPI.DTWAIN_SysInitialize();
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
            public long Quality = 100;
        }
        private class ScannerProperties
        {
            public string ProductName;
            public bool IsSupportFeeder;
            public bool IsSupportDuplex;
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
            NumericUpDown resolutionInput, TrackBar brightnessInput, Label brightnessLabel, TrackBar contrastInput, Label contrastLabel, TrackBar qualityInput, Label qualityLabel,
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
                DoGetScanWorker.ReportProgress(1, "DTWAIN_StartThread");
                TwainAPI.DTWAIN_StartThread(DTWAINHelper.DTWAINThread);
                DoGetScanWorker.ReportProgress(2, "DTWAIN_StartTwainSession");
                //StartTwainSession first argument parent window, second argument DLL file;
                TwainAPI.DTWAIN_StartTwainSession(IntPtr.Zero,null);
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
                TwainAPI.DTWAIN_EndTwainSession();
                DoGetScanWorker.ReportProgress(99, "DTWAIN_EndThread");
                TwainAPI.DTWAIN_EndThread(DTWAINHelper.DTWAINThread);
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
                }
                if (e.ProgressPercentage > 0)
                {
                    this.scannerLabel.Text = "(" + e.ProgressPercentage.ToString() + "% " + message + ")";
                    Debug.WriteLine("(" + e.ProgressPercentage.ToString() + "% " + message + ")");
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
                DoSetScanWorker.ReportProgress(1, "DTWAIN_StartThread");
                TwainAPI.DTWAIN_StartThread(DTWAINHelper.DTWAINThread);
                DoSetScanWorker.ReportProgress(2, "DTWAIN_StartTwainSession");
                //StartTwainSession first argument parent window, second argument DLL file;
                TwainAPI.DTWAIN_StartTwainSession(IntPtr.Zero, null);
                DTWAINHelper.IsDTWAINBusy = true;

                DoSetScanWorker.ReportProgress(3, "DTWAIN_SelectSourceByName");
                DTWAIN_SOURCE PtrSouce = TwainAPI.DTWAIN_SelectSourceByName(args);

                if (PtrSouce != IntPtr.Zero)
                {
                    //Source Setting
                    DoSetScanWorker.ReportProgress(4, "DTWAIN_IsFeederDuplexSupported");
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
                }
                else
                {
                    DoSetScanWorker.ReportProgress(-1, "SCANNER_NOT_FOUND");
                }

                DoSetScanWorker.ReportProgress(98, "DTWAIN_EndTwainSession");
                TwainAPI.DTWAIN_EndTwainSession();
                DoSetScanWorker.ReportProgress(99, "DTWAIN_EndThread");
                TwainAPI.DTWAIN_EndThread(DTWAINHelper.DTWAINThread);
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
                }
                if (e.ProgressPercentage > 0)
                {
                    this.scannerLabel.Text = "(" + e.ProgressPercentage.ToString() + "% " + message + ")";
                    Debug.WriteLine("(" + e.ProgressPercentage.ToString() + "% " + message + ")");
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
                if(this.formStatus != null)
                {
                    this.formStatus.Dispose();
                }
                this.formStatus = new Form2();
                this.formStatus.Show();
                this.formStatus.Invoke((MethodInvoker)delegate
                {
                    this.formStatus.setStatus();
                    this.formStatus.TopLevel = true;
                    this.formStatus.TopMost = true;
                });
                DoScanReportProgressInterceptor(1, "DTWAIN_StartThread");
                TwainAPI.DTWAIN_StartThread(DTWAINHelper.DTWAINThread);
                DoScanReportProgressInterceptor(2, "DTWAIN_StartTwainSession");
                //StartTwainSession first argument parent window, second argument DLL file;
                TwainAPI.DTWAIN_StartTwainSession(this.formStatus.Handle, null);
                DTWAINHelper.IsDTWAINBusy = true;
                DTWAINHelper.KeepScanning = true;
                DoScanReportProgressInterceptor(2, "DTWAIN_SetCallback");
                TwainAPI.DTWAIN_EnableMsgNotify(1);
                TwainAPI.DTWAIN_SetCallback(this.AcquireCallback, 0);

                DoScanReportProgressInterceptor(3, "DTWAIN_SelectSourceByName");
                DTWAIN_SOURCE PtrSouce = TwainAPI.DTWAIN_SelectSourceByName(CurrentSelectedScanner);

                if (PtrSouce != IntPtr.Zero)
                {
                    DoScanReportProgressInterceptor(4, "DTWAIN_OpenSource");
                    if (TwainAPI.DTWAIN_OpenSource(PtrSouce) == 1)
                    {
                        //Source Setting
                        /*DoScanWorker.ReportProgress(5, "DTWAIN_IsIndicatorSupported");
                        if (TwainAPI.DTWAIN_IsIndicatorSupported(PtrSouce) == 1)
                        {
                            DoScanWorker.ReportProgress(6, "DTWAIN_EnableIndicator");
                            TwainAPI.DTWAIN_EnableIndicator(PtrSouce, 1);
                        }*/
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
                        //Paper Setting
                        DoScanReportProgressInterceptor(11, "DTWAIN_SetPaperSize");
                        TwainAPI.DTWAIN_SetPaperSize(PtrSouce, args.Paper, args.Paper == 0 ? 0 : 1);
                        //Resolution
                        DoScanReportProgressInterceptor(12, "DTWAIN_SetResolution");
                        TwainAPI.DTWAIN_SetResolution(PtrSouce, args.Resolution);
                        //Brightness
                        DoScanReportProgressInterceptor(13, "DTWAIN_SetBrightness");
                        TwainAPI.DTWAIN_SetBrightness(PtrSouce, args.Brightness);
                        //Contrast
                        DoScanReportProgressInterceptor(14, "DTWAIN_SetContrast");
                        TwainAPI.DTWAIN_SetContrast(PtrSouce, args.Contrast);

                        //Acquisition Array
                        //Inside Acquistion Array are DIB Array
                        DoScanReportProgressInterceptor(15, "DTWAIN_CreateAcquisitionArray");
                        DTWAIN_ARRAY AcqArray = TwainAPI.DTWAIN_CreateAcquisitionArray();
                        int pStatus = 0;
                        DoScanReportProgressInterceptor(16, "DTWAIN_AcquireNativeEx");
                        int AcquireCodeResult = 0;
                        AcquireCodeResult = TwainAPI.DTWAIN_AcquireNativeEx(
                            PtrSouce,
                            args.Color,
                            TwainAPI.DTWAIN_ACQUIREALL,
                            0,
                            0,
                            AcqArray,
                            ref pStatus
                        );
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
                                    this.ViewScanImages.AddImage(IAcq, IDib, StreamScan);
                                    String Base64Scan = "data:image/jpeg;base64," + Convert.ToBase64String(StreamScan.ToArray());
                                    JDib.Add(Base64Scan);
                                    BmpScan.Dispose();
                                    /*Debug.WriteLine(Base64Scan);*/
                                }
                                JAcq.Add(JDib);
                            }
                            result["data"] = JAcq;
                        }
                        else
                        {
                            DoScanReportProgressInterceptor(-1, "ACQ_FAILED");
                        }

                        DoScanReportProgressInterceptor(96, "DTWAIN_DestroyAcquisitionArray");
                        TwainAPI.DTWAIN_DestroyAcquisitionArray(AcqArray, 1);
                        DoScanReportProgressInterceptor(97, "DTWAIN_CloseSource");
                        TwainAPI.DTWAIN_CloseSource(PtrSouce);
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
                TwainAPI.DTWAIN_EndTwainSession();
                DoScanReportProgressInterceptor(99, "DTWAIN_EndThread");
                TwainAPI.DTWAIN_EndThread(DTWAINHelper.DTWAINThread);
                DTWAINHelper.IsDTWAINBusy = false;
                DTWAINHelper.KeepScanning = false;
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
                }
                if (e.ProgressPercentage == 0)
                {
                    Debug.WriteLine("Acquire Callback : " + code);
                    result["request"] = "onScanCallback";
                    result["status"] = code;
                }
                if (e.ProgressPercentage > 0)
                {
                    this.form1.Invoke((MethodInvoker)delegate
                    {
                        this.scannerLabel.Text = DTWAINHelper.SelectedScanner + " (" + e.ProgressPercentage.ToString() + "% " + code + ")";
                    });
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
                    result["request"] = "onScanError";
                    result["error"] = code;
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
                    if (e.Result is string && socket.IsAvailable)
                        socket.Send((string)e.Result);
                    socket = null;
                }
            };
        }
        private void DoScanReportProgressInterceptor(int ProgressPercentage, String code)
        {
            if (DoScanWorker.IsBusy)
            {
                DoScanWorker.ReportProgress(ProgressPercentage, code);
            }
            if (this.formStatus != null && this.formStatus.IsHandleCreated && this.formStatus.Visible)
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
                args.Resolution = int.Parse(this.resolutionInput.Value.ToString());
                args.Brightness = this.brightnessInput.Value;
                args.Contrast = this.contrastInput.Value;
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
    }
}
