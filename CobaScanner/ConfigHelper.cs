using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CobaScanner
{
    internal class ConfigHelper
    {
        public class ConfigApp
        {
            public string Scanner = "";
            public int Source = 0;
            public int Paper = 0;
            public int Color = 0;
            public int Resolution = 200;
            public int Brightness = 0;
            public int Contrast = 0;

            public int MaxAcq = 0;
            public int MaxPage = 0;
            public bool SourceUI = false;
            public bool SourceIndicator = false;

            public int Quality = 100;

            public int Port = 5678;
            public List<string> WhiteList = new List<string>();
        }

        private Form1 form1;
        private ListBox ScannerBox;
        private ComboBox SourceBox;
        private ComboBox PaperBox;
        private ComboBox ColorBox;
        private NumericUpDown ResolutionInput;
        private TrackBar BrightnessInput;
        private Label BrightnessLabel;
        private TrackBar ContrastInput;
        private Label ContrastLabel;
        private NumericUpDown MaxAcqInput;
        private NumericUpDown MaxPageInput;
        private CheckBox SourceUICheckBox;
        private CheckBox SourceIndicatorCheckBox;
        private TrackBar QualityInput;
        private Label QualityLabel;

        private NumericUpDown PortInput;
        private ListBox WhiteListBox;

        public static ConfigApp Conf = new ConfigApp();
        private static string ConfigFileLocation = "";
        public ConfigHelper(
            Form1 form1,
            ListBox scannerBox, ComboBox sourceBox, ComboBox paperBox, ComboBox colorBox, NumericUpDown resolutionInput,
            TrackBar brightnessInput, Label brightnessLabel, TrackBar contrastInput, Label contrastLabel,
            NumericUpDown maxAcqInput, NumericUpDown maxPageInput, CheckBox sourceUiCheck, CheckBox sourceIndicatorCheck,
            TrackBar qualityInput, Label qualityLabel,
            NumericUpDown portInput, ListBox whiteListBox
            )
        {
            this.form1 = form1;
            this.SourceBox = sourceBox;
            this.PaperBox = paperBox;
            this.ColorBox = colorBox;
            this.ResolutionInput = resolutionInput;
            this.BrightnessInput = brightnessInput;
            this.BrightnessLabel = brightnessLabel;
            this.ContrastInput = contrastInput;
            this.ContrastLabel = contrastLabel;
            this.MaxAcqInput = maxAcqInput;
            this.MaxPageInput = maxPageInput;
            this.SourceUICheckBox = sourceUiCheck;
            this.SourceIndicatorCheckBox = sourceIndicatorCheck;
            this.QualityInput = qualityInput;
            this.QualityLabel = qualityLabel;

            this.PortInput = portInput;
            this.WhiteListBox = whiteListBox;
            string FileLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string FolderLocation = Path.GetDirectoryName(FileLocation)!;
            ConfigHelper.ConfigFileLocation = System.IO.Path.Combine(FolderLocation, "config.json");
            this.LoadConfig();
        }

        private ConfigApp LoadConfig()
        {
            if (!File.Exists(ConfigHelper.ConfigFileLocation))
            {
                this.ResetConfig();
            }
            else
            {
                JsonNode conf = JsonNode.Parse(File.ReadAllText(ConfigHelper.ConfigFileLocation))!;
                ConfigHelper.Conf = new ConfigApp();
                ConfigHelper.Conf.Scanner = (String)(conf!["scanner"]! ?? "");

                ConfigHelper.Conf.Source = (int)(conf!["source"]! ?? 0);
                ConfigHelper.Conf.Paper = (int)(conf!["paper"]! ?? 0);
                ConfigHelper.Conf.Color = (int)(conf!["color"]! ?? 0);

                ConfigHelper.Conf.Resolution = (int)(conf!["resolution"]! ?? 200);
                ConfigHelper.Conf.Brightness = (int)(conf!["brightness"]! ?? 0);
                ConfigHelper.Conf.Contrast = (int)(conf!["contrast"]! ?? 0);
                ConfigHelper.Conf.MaxAcq = (int)(conf!["max_acq"]! ?? 0);
                ConfigHelper.Conf.MaxPage = (int)(conf!["max_page"]! ?? 0);
                ConfigHelper.Conf.SourceUI = (bool)(conf!["source_ui"]! ?? false);
                ConfigHelper.Conf.SourceIndicator = (bool)(conf!["source_indicator"]! ?? false);
                ConfigHelper.Conf.Quality = (int)(conf!["quality"]! ?? 80);

                ConfigHelper.Conf.Port = (int)conf!["port"]!;
                ConfigHelper.Conf.WhiteList.Clear();
                JsonArray whitelists = (JsonArray)conf!["whitelists"]!;
                foreach (String whitelist in whitelists)
                {
                    ConfigHelper.Conf.WhiteList.Add(whitelist);
                }
                Debug.WriteLine(ConfigHelper.Conf.ToString());
            }

            this.ColorBox.SelectedIndex = ConfigHelper.Conf.Color;
            this.ResolutionInput.Value = ConfigHelper.Conf.Resolution;
            this.BrightnessInput.Value = ConfigHelper.Conf.Brightness;
            this.BrightnessLabel.Text = ConfigHelper.Conf.Brightness.ToString();
            this.ContrastInput.Value = ConfigHelper.Conf.Contrast;
            this.ContrastLabel.Text = ConfigHelper.Conf.Contrast.ToString();
            this.MaxAcqInput.Value = ConfigHelper.Conf.MaxAcq;
            this.MaxPageInput.Value = ConfigHelper.Conf.MaxPage;
            this.SourceUICheckBox.Checked = ConfigHelper.Conf.SourceUI;
            this.SourceIndicatorCheckBox.Checked = ConfigHelper.Conf.SourceIndicator;
            this.QualityInput.Value = ConfigHelper.Conf.Quality;
            this.QualityLabel.Text = ConfigHelper.Conf.Quality.ToString();

            this.PortInput.Value = ConfigHelper.Conf.Port;
            this.WhiteListBox.Items.Clear();
            foreach (String whitelist in ConfigHelper.Conf.WhiteList)
            {
                this.WhiteListBox.Items.Add(whitelist);
            }

            return ConfigHelper.Conf;
        }

        private ConfigApp ResetConfig()
        {
            ConfigHelper.Conf = new ConfigApp();
            this.SaveConfigToFile();
            return Conf;
        }

        private void SaveConfigToFile()
        {
            JsonObject conf = new JsonObject();

            conf["scanner"] = ConfigHelper.Conf.Scanner;

            conf["source"] = ConfigHelper.Conf.Source;
            conf["paper"] = ConfigHelper.Conf.Paper;
            conf["color"] = ConfigHelper.Conf.Color;
            conf["resolution"] = ConfigHelper.Conf.Resolution;
            conf["brightness"] = ConfigHelper.Conf.Brightness;
            conf["contrast"] = ConfigHelper.Conf.Contrast;
            conf["max_acq"] = ConfigHelper.Conf.MaxAcq;
            conf["max_page"] = ConfigHelper.Conf.MaxPage;
            conf["source_ui"] = ConfigHelper.Conf.SourceUI;
            conf["source_indicator"] = ConfigHelper.Conf.SourceIndicator;
            conf["quality"] = ConfigHelper.Conf.Quality;

            conf["port"] = ConfigHelper.Conf.Port;
            JsonArray whiteLists = new JsonArray();
            foreach (String whiteList in ConfigHelper.Conf.WhiteList)
            {
                whiteLists.Add<String>(whiteList);
            }
            conf["whitelists"] = whiteLists;
            File.WriteAllText(ConfigHelper.ConfigFileLocation, conf.ToJsonString());
        }

        public void SaveAllConfig(String SelectedScanner)
        {
            ConfigHelper.Conf.Scanner = SelectedScanner;
            if (SelectedScanner.Length > 0)
            {
                ConfigHelper.Conf.Source = this.SourceBox.SelectedIndex;
                ConfigHelper.Conf.Paper = this.PaperBox.SelectedIndex;
            }
            else
            {
                ConfigHelper.Conf.Source = 0;
                ConfigHelper.Conf.Paper = 0;
            }
            ConfigHelper.Conf.Resolution = (int)this.ResolutionInput.Value;
            ConfigHelper.Conf.Color = this.ColorBox.SelectedIndex;
            ConfigHelper.Conf.Brightness = this.BrightnessInput.Value;
            ConfigHelper.Conf.Contrast = this.ContrastInput.Value;
            ConfigHelper.Conf.MaxAcq = (int)this.MaxAcqInput.Value;
            ConfigHelper.Conf.MaxPage = (int)this.MaxPageInput.Value;
            ConfigHelper.Conf.SourceUI = this.SourceUICheckBox.Checked;
            ConfigHelper.Conf.SourceIndicator = this.SourceIndicatorCheckBox.Checked;
            ConfigHelper.Conf.Quality = this.QualityInput.Value;

            ConfigHelper.Conf.Port = (int)this.PortInput.Value;
            this.SaveWhiteList();
        }

        public void SaveWhiteList()
        {
            ConfigHelper.Conf.WhiteList.Clear();
            this.form1.Invoke((MethodInvoker)delegate
            {
                foreach (String whitelist in this.WhiteListBox.Items)
                {
                    ConfigHelper.Conf.WhiteList.Add(whitelist);
                }
            });
            this.SaveConfigToFile();
        }
    }
}
