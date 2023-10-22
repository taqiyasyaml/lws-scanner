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

        private NumericUpDown PortInput;
        private ListBox WhiteListBox;

        public static ConfigApp Conf = new ConfigApp();
        private static string ConfigFileLocation = "";
        public ConfigHelper(
            Form1 form1,
            ListBox scannerBox, ComboBox sourceBox, ComboBox paperBox, ComboBox colorBox, NumericUpDown resolutionInput,
            TrackBar brightnessInput, Label brightnessLabel, TrackBar contrastInput, Label contrastLabel,
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
                ConfigHelper.Conf.Scanner = (String)conf!["scanner"]!;

                ConfigHelper.Conf.Source = (int)conf!["source"]!;
                ConfigHelper.Conf.Paper = (int)conf!["paper"]!;
                ConfigHelper.Conf.Color = (int)conf!["color"]!;

                ConfigHelper.Conf.Resolution = (int)conf!["resolution"]!;
                ConfigHelper.Conf.Brightness = (int)conf!["brightness"]!;
                ConfigHelper.Conf.Contrast = (int)conf!["contrast"]!;

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
