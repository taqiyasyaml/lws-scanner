namespace CobaScanner
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            label1 = new Label();
            scannerBox = new ListBox();
            scannerGetButton = new Button();
            scannerLabel = new Label();
            label2 = new Label();
            sourceBox = new ComboBox();
            paperBox = new ComboBox();
            label3 = new Label();
            colorBox = new ComboBox();
            label4 = new Label();
            resolutionInput = new NumericUpDown();
            label5 = new Label();
            label6 = new Label();
            brightnessInput = new TrackBar();
            brightnessLabel = new Label();
            contrastLabel = new Label();
            contrastInput = new TrackBar();
            label8 = new Label();
            startScanButton = new Button();
            notifyIcon1 = new NotifyIcon(components);
            resetSettingButton = new Button();
            stopScanButton = new Button();
            label7 = new Label();
            portInput = new NumericUpDown();
            startStopWSButton = new Button();
            whiteListBox = new ListBox();
            label9 = new Label();
            deleteWhiteList = new Button();
            saveSettingButton = new Button();
            startupButton = new Button();
            desktopButton = new Button();
            label10 = new Label();
            scanPicture = new PictureBox();
            label11 = new Label();
            previousScanButton = new Button();
            nextScanButton = new Button();
            scanImagePositionLabel = new Label();
            qualityLabel = new Label();
            qualityInput = new TrackBar();
            label13 = new Label();
            label12 = new Label();
            maxAcq = new NumericUpDown();
            label14 = new Label();
            maxPage = new NumericUpDown();
            sourceUiCheck = new CheckBox();
            sourceIndicatorCheck = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)resolutionInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)brightnessInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)contrastInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)portInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)scanPicture).BeginInit();
            ((System.ComponentModel.ISupportInitialize)qualityInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)maxAcq).BeginInit();
            ((System.ComponentModel.ISupportInitialize)maxPage).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(10, 10);
            label1.Name = "label1";
            label1.Size = new Size(58, 15);
            label1.TabIndex = 0;
            label1.Text = "Scanner :";
            // 
            // scannerBox
            // 
            scannerBox.FormattingEnabled = true;
            scannerBox.ItemHeight = 15;
            scannerBox.Location = new Point(10, 40);
            scannerBox.Name = "scannerBox";
            scannerBox.Size = new Size(360, 199);
            scannerBox.TabIndex = 2;
            scannerBox.SelectedIndexChanged += scannerBox_SelectedIndexChanged;
            // 
            // scannerGetButton
            // 
            scannerGetButton.Location = new Point(245, 10);
            scannerGetButton.Name = "scannerGetButton";
            scannerGetButton.Size = new Size(125, 25);
            scannerGetButton.TabIndex = 1;
            scannerGetButton.Text = "Refresh Scanner List";
            scannerGetButton.UseVisualStyleBackColor = true;
            scannerGetButton.Click += scannerGetButton_Click;
            // 
            // scannerLabel
            // 
            scannerLabel.AutoSize = true;
            scannerLabel.Location = new Point(10, 250);
            scannerLabel.Name = "scannerLabel";
            scannerLabel.Size = new Size(115, 15);
            scannerLabel.TabIndex = 3;
            scannerLabel.Text = "No Selected Scanner";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(10, 275);
            label2.Name = "label2";
            label2.Size = new Size(41, 15);
            label2.TabIndex = 4;
            label2.Text = "Souce";
            // 
            // sourceBox
            // 
            sourceBox.DropDownStyle = ComboBoxStyle.DropDownList;
            sourceBox.FormattingEnabled = true;
            sourceBox.Items.AddRange(new object[] { "Flatbed" });
            sourceBox.Location = new Point(120, 275);
            sourceBox.Name = "sourceBox";
            sourceBox.Size = new Size(250, 23);
            sourceBox.TabIndex = 5;
            // 
            // paperBox
            // 
            paperBox.DropDownStyle = ComboBoxStyle.DropDownList;
            paperBox.Enabled = false;
            paperBox.FormattingEnabled = true;
            paperBox.Items.AddRange(new object[] { "Default", "A4 Letter", "B5 Letter", "US Letter", "US Legal", "A5", "B4", "B6", "US Ledger", "US Executive", "A3", "B3", "A6", "C4", "C5", "C6", "4 A0", "2 A0", "A0", "A1", "A2", "A4", "A7", "A8", "A9", "A10", "ISO B0", "ISO B1", "ISO B2", "ISO B3", "ISO B4", "ISO B5", "ISO B6", "ISO B7", "ISO B8", "ISO B9", "ISO B10", "JIS B0", "JIS B1", "JIS B2", "JIS B3", "JIS B4", "JIS B5", "JIS B6", "JIS B7", "JIS B8", "JIS B9", "JIS B10", "C0", "C1", "C2", "C3", "C7", "C8", "C9", "C10", "US Statement", "Business Card" });
            paperBox.Location = new Point(120, 305);
            paperBox.Name = "paperBox";
            paperBox.Size = new Size(250, 23);
            paperBox.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(10, 305);
            label3.Name = "label3";
            label3.Size = new Size(65, 15);
            label3.TabIndex = 6;
            label3.Text = "Paper Size";
            // 
            // colorBox
            // 
            colorBox.DropDownStyle = ComboBoxStyle.DropDownList;
            colorBox.FormattingEnabled = true;
            colorBox.Items.AddRange(new object[] { "Default", "Black and White", "Grayscale", "RGB Color", "CMYK Color" });
            colorBox.Location = new Point(120, 335);
            colorBox.Name = "colorBox";
            colorBox.Size = new Size(250, 23);
            colorBox.TabIndex = 9;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label4.Location = new Point(10, 335);
            label4.Name = "label4";
            label4.Size = new Size(36, 15);
            label4.TabIndex = 8;
            label4.Text = "Color";
            // 
            // resolutionInput
            // 
            resolutionInput.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            resolutionInput.Location = new Point(250, 364);
            resolutionInput.Maximum = new decimal(new int[] { 12800, 0, 0, 0 });
            resolutionInput.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            resolutionInput.Name = "resolutionInput";
            resolutionInput.Size = new Size(120, 23);
            resolutionInput.TabIndex = 11;
            resolutionInput.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label5.Location = new Point(10, 370);
            label5.Name = "label5";
            label5.Size = new Size(66, 15);
            label5.TabIndex = 10;
            label5.Text = "Resolution";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label6.Location = new Point(10, 395);
            label6.Name = "label6";
            label6.Size = new Size(66, 15);
            label6.TabIndex = 12;
            label6.Text = "Brightness";
            // 
            // brightnessInput
            // 
            brightnessInput.LargeChange = 10;
            brightnessInput.Location = new Point(120, 395);
            brightnessInput.Maximum = 1000;
            brightnessInput.Minimum = -1000;
            brightnessInput.Name = "brightnessInput";
            brightnessInput.Size = new Size(250, 45);
            brightnessInput.TabIndex = 13;
            brightnessInput.TickStyle = TickStyle.None;
            brightnessInput.Scroll += brightnessInput_Scroll;
            // 
            // brightnessLabel
            // 
            brightnessLabel.AutoSize = true;
            brightnessLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            brightnessLabel.Location = new Point(10, 415);
            brightnessLabel.Name = "brightnessLabel";
            brightnessLabel.Size = new Size(13, 15);
            brightnessLabel.TabIndex = 14;
            brightnessLabel.Text = "0";
            // 
            // contrastLabel
            // 
            contrastLabel.AutoSize = true;
            contrastLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            contrastLabel.Location = new Point(10, 455);
            contrastLabel.Name = "contrastLabel";
            contrastLabel.Size = new Size(13, 15);
            contrastLabel.TabIndex = 17;
            contrastLabel.Text = "0";
            // 
            // contrastInput
            // 
            contrastInput.LargeChange = 10;
            contrastInput.Location = new Point(120, 440);
            contrastInput.Maximum = 1000;
            contrastInput.Minimum = -1000;
            contrastInput.Name = "contrastInput";
            contrastInput.Size = new Size(250, 45);
            contrastInput.TabIndex = 16;
            contrastInput.TickStyle = TickStyle.None;
            contrastInput.Scroll += contrastInput_Scroll;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label8.Location = new Point(10, 440);
            label8.Name = "label8";
            label8.Size = new Size(54, 15);
            label8.TabIndex = 15;
            label8.Text = "Contrast";
            // 
            // startScanButton
            // 
            startScanButton.Location = new Point(135, 630);
            startScanButton.Name = "startScanButton";
            startScanButton.Size = new Size(110, 23);
            startScanButton.TabIndex = 19;
            startScanButton.Text = "Scanner Test";
            startScanButton.UseVisualStyleBackColor = true;
            startScanButton.Click += startScanButton_Click;
            // 
            // notifyIcon1
            // 
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "LWS Scanner";
            notifyIcon1.Visible = true;
            notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;
            // 
            // resetSettingButton
            // 
            resetSettingButton.Location = new Point(10, 630);
            resetSettingButton.Name = "resetSettingButton";
            resetSettingButton.Size = new Size(110, 23);
            resetSettingButton.TabIndex = 18;
            resetSettingButton.Text = "Reset Settings";
            resetSettingButton.UseVisualStyleBackColor = true;
            resetSettingButton.Click += resetSettingButton_Click;
            // 
            // stopScanButton
            // 
            stopScanButton.Enabled = false;
            stopScanButton.Location = new Point(260, 630);
            stopScanButton.Name = "stopScanButton";
            stopScanButton.Size = new Size(110, 23);
            stopScanButton.TabIndex = 20;
            stopScanButton.Text = "Cancel Next Page";
            stopScanButton.UseVisualStyleBackColor = true;
            stopScanButton.Click += stopScanButton_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label7.Location = new Point(400, 660);
            label7.Name = "label7";
            label7.Size = new Size(99, 15);
            label7.TabIndex = 21;
            label7.Text = "WebSocket Port";
            // 
            // portInput
            // 
            portInput.Location = new Point(525, 660);
            portInput.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            portInput.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            portInput.Name = "portInput";
            portInput.Size = new Size(110, 23);
            portInput.TabIndex = 22;
            portInput.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // startStopWSButton
            // 
            startStopWSButton.Location = new Point(650, 660);
            startStopWSButton.Name = "startStopWSButton";
            startStopWSButton.Size = new Size(110, 23);
            startStopWSButton.TabIndex = 23;
            startStopWSButton.Text = "Start";
            startStopWSButton.UseVisualStyleBackColor = true;
            startStopWSButton.Click += startStopWSButton_Click;
            // 
            // whiteListBox
            // 
            whiteListBox.FormattingEnabled = true;
            whiteListBox.ItemHeight = 15;
            whiteListBox.Location = new Point(400, 720);
            whiteListBox.Name = "whiteListBox";
            whiteListBox.Size = new Size(360, 79);
            whiteListBox.TabIndex = 25;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label9.Location = new Point(400, 690);
            label9.Name = "label9";
            label9.Size = new Size(111, 15);
            label9.TabIndex = 24;
            label9.Text = "White List Origins :";
            // 
            // deleteWhiteList
            // 
            deleteWhiteList.Location = new Point(650, 690);
            deleteWhiteList.Name = "deleteWhiteList";
            deleteWhiteList.Size = new Size(110, 23);
            deleteWhiteList.TabIndex = 26;
            deleteWhiteList.Text = "Delete";
            deleteWhiteList.UseVisualStyleBackColor = true;
            deleteWhiteList.Click += deleteWhiteList_Click;
            // 
            // saveSettingButton
            // 
            saveSettingButton.Location = new Point(10, 720);
            saveSettingButton.Name = "saveSettingButton";
            saveSettingButton.Size = new Size(360, 30);
            saveSettingButton.TabIndex = 27;
            saveSettingButton.Text = "Save Setting";
            saveSettingButton.UseVisualStyleBackColor = true;
            saveSettingButton.Click += saveSettingButton_Click;
            // 
            // startupButton
            // 
            startupButton.Location = new Point(10, 760);
            startupButton.Name = "startupButton";
            startupButton.Size = new Size(175, 30);
            startupButton.TabIndex = 28;
            startupButton.Text = "Disable StartUp (Current User)";
            startupButton.UseVisualStyleBackColor = true;
            startupButton.Click += startupButton_Click;
            // 
            // desktopButton
            // 
            desktopButton.Location = new Point(195, 760);
            desktopButton.Name = "desktopButton";
            desktopButton.Size = new Size(175, 30);
            desktopButton.TabIndex = 29;
            desktopButton.Text = "Delete Shortcut (Current User)";
            desktopButton.UseVisualStyleBackColor = true;
            desktopButton.Click += desktopButton_Click;
            // 
            // label10
            // 
            label10.ForeColor = SystemColors.ControlDarkDark;
            label10.Location = new Point(12, 800);
            label10.Name = "label10";
            label10.Size = new Size(748, 50);
            label10.TabIndex = 30;
            label10.Text = "Listyawan WebSocket Scanner 2023\r\nMade with love by <taqiyasyaml@gmail.com>\r\nThis app use .NET, dynarithmic.com DLL, and Fleck DLL";
            label10.TextAlign = ContentAlignment.MiddleCenter;
            label10.Click += label10_Click;
            // 
            // scanPicture
            // 
            scanPicture.BorderStyle = BorderStyle.FixedSingle;
            scanPicture.Location = new Point(375, 60);
            scanPicture.Name = "scanPicture";
            scanPicture.Size = new Size(400, 564);
            scanPicture.SizeMode = PictureBoxSizeMode.Zoom;
            scanPicture.TabIndex = 32;
            scanPicture.TabStop = false;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label11.Location = new Point(375, 40);
            label11.Name = "label11";
            label11.Size = new Size(122, 15);
            label11.TabIndex = 31;
            label11.Text = "Last Scanned Images";
            // 
            // previousScanButton
            // 
            previousScanButton.Location = new Point(375, 630);
            previousScanButton.Name = "previousScanButton";
            previousScanButton.Size = new Size(50, 23);
            previousScanButton.TabIndex = 33;
            previousScanButton.Text = "<";
            previousScanButton.UseVisualStyleBackColor = true;
            previousScanButton.Click += previousScanButton_Click;
            // 
            // nextScanButton
            // 
            nextScanButton.Location = new Point(725, 630);
            nextScanButton.Name = "nextScanButton";
            nextScanButton.Size = new Size(50, 23);
            nextScanButton.TabIndex = 34;
            nextScanButton.Text = ">";
            nextScanButton.UseVisualStyleBackColor = true;
            nextScanButton.Click += nextScanButton_Click;
            // 
            // scanImagePositionLabel
            // 
            scanImagePositionLabel.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            scanImagePositionLabel.Location = new Point(430, 630);
            scanImagePositionLabel.Name = "scanImagePositionLabel";
            scanImagePositionLabel.Size = new Size(290, 23);
            scanImagePositionLabel.TabIndex = 35;
            scanImagePositionLabel.Text = "label12";
            scanImagePositionLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // qualityLabel
            // 
            qualityLabel.AutoSize = true;
            qualityLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            qualityLabel.Location = new Point(10, 600);
            qualityLabel.Name = "qualityLabel";
            qualityLabel.Size = new Size(13, 15);
            qualityLabel.TabIndex = 38;
            qualityLabel.Text = "0";
            // 
            // qualityInput
            // 
            qualityInput.Location = new Point(120, 585);
            qualityInput.Maximum = 100;
            qualityInput.Name = "qualityInput";
            qualityInput.Size = new Size(250, 45);
            qualityInput.TabIndex = 37;
            qualityInput.TickStyle = TickStyle.None;
            qualityInput.Value = 100;
            qualityInput.Scroll += qualityInput_Scroll;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point);
            label13.Location = new Point(10, 585);
            label13.Name = "label13";
            label13.Size = new Size(108, 13);
            label13.TabIndex = 36;
            label13.Text = "Quality (After Scan)";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label12.Location = new Point(10, 480);
            label12.Name = "label12";
            label12.Size = new Size(166, 15);
            label12.TabIndex = 39;
            label12.Text = "Max Acquistion (0 is Default)";
            // 
            // maxAcq
            // 
            maxAcq.Location = new Point(250, 475);
            maxAcq.Name = "maxAcq";
            maxAcq.Size = new Size(120, 23);
            maxAcq.TabIndex = 40;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label14.Location = new Point(10, 510);
            label14.Name = "label14";
            label14.Size = new Size(135, 15);
            label14.TabIndex = 41;
            label14.Text = "Max Page (0 is Default)";
            // 
            // maxPage
            // 
            maxPage.Location = new Point(250, 505);
            maxPage.Name = "maxPage";
            maxPage.Size = new Size(120, 23);
            maxPage.TabIndex = 42;
            // 
            // sourceUiCheck
            // 
            sourceUiCheck.AutoSize = true;
            sourceUiCheck.Location = new Point(10, 535);
            sourceUiCheck.Name = "sourceUiCheck";
            sourceUiCheck.Size = new Size(166, 19);
            sourceUiCheck.TabIndex = 43;
            sourceUiCheck.Text = "Show Driver UI Acquisition";
            sourceUiCheck.UseVisualStyleBackColor = true;
            // 
            // sourceIndicatorCheck
            // 
            sourceIndicatorCheck.AutoSize = true;
            sourceIndicatorCheck.Location = new Point(10, 560);
            sourceIndicatorCheck.Name = "sourceIndicatorCheck";
            sourceIndicatorCheck.Size = new Size(139, 19);
            sourceIndicatorCheck.TabIndex = 44;
            sourceIndicatorCheck.Text = "Show Driver Indicator";
            sourceIndicatorCheck.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 861);
            Controls.Add(sourceIndicatorCheck);
            Controls.Add(sourceUiCheck);
            Controls.Add(label14);
            Controls.Add(maxPage);
            Controls.Add(label12);
            Controls.Add(maxAcq);
            Controls.Add(qualityLabel);
            Controls.Add(qualityInput);
            Controls.Add(label13);
            Controls.Add(scanImagePositionLabel);
            Controls.Add(nextScanButton);
            Controls.Add(previousScanButton);
            Controls.Add(label11);
            Controls.Add(scanPicture);
            Controls.Add(label10);
            Controls.Add(desktopButton);
            Controls.Add(startupButton);
            Controls.Add(saveSettingButton);
            Controls.Add(deleteWhiteList);
            Controls.Add(label9);
            Controls.Add(whiteListBox);
            Controls.Add(startStopWSButton);
            Controls.Add(portInput);
            Controls.Add(label7);
            Controls.Add(stopScanButton);
            Controls.Add(resetSettingButton);
            Controls.Add(startScanButton);
            Controls.Add(contrastLabel);
            Controls.Add(contrastInput);
            Controls.Add(label8);
            Controls.Add(brightnessLabel);
            Controls.Add(brightnessInput);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(resolutionInput);
            Controls.Add(colorBox);
            Controls.Add(label4);
            Controls.Add(paperBox);
            Controls.Add(label3);
            Controls.Add(sourceBox);
            Controls.Add(label2);
            Controls.Add(scannerLabel);
            Controls.Add(scannerGetButton);
            Controls.Add(scannerBox);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            Text = "LWS Scanner";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            Resize += Form1_Resize;
            ((System.ComponentModel.ISupportInitialize)resolutionInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)brightnessInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)contrastInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)portInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)scanPicture).EndInit();
            ((System.ComponentModel.ISupportInitialize)qualityInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)maxAcq).EndInit();
            ((System.ComponentModel.ISupportInitialize)maxPage).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ListBox scannerBox;
        private Button scannerGetButton;
        private Label scannerLabel;
        private Label label2;
        private ComboBox sourceBox;
        private ComboBox paperBox;
        private Label label3;
        private ComboBox colorBox;
        private Label label4;
        private NumericUpDown resolutionInput;
        private Label label5;
        private Label label6;
        private TrackBar brightnessInput;
        private Label brightnessLabel;
        private Label contrastLabel;
        private TrackBar contrastInput;
        private Label label8;
        private Button startScanButton;
        private NotifyIcon notifyIcon1;
        private Button resetSettingButton;
        private Button stopScanButton;
        private Label label7;
        private NumericUpDown portInput;
        private Button startStopWSButton;
        private ListBox whiteListBox;
        private Label label9;
        private Button deleteWhiteList;
        private Button saveSettingButton;
        private Button startupButton;
        private Button desktopButton;
        private Label label10;
        private PictureBox scanPicture;
        private Label label11;
        private Button previousScanButton;
        private Button nextScanButton;
        private Label scanImagePositionLabel;
        private Label qualityLabel;
        private TrackBar qualityInput;
        private Label label13;
        private Label label12;
        private NumericUpDown maxAcq;
        private Label label14;
        private NumericUpDown maxPage;
        private CheckBox sourceUiCheck;
        private CheckBox sourceIndicatorCheck;
    }
}