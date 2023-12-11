using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CobaScanner
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public void setStatus(String status)
        {
            this.statusLabel.Text = status;
        }

        public void setStatus()
        {
            this.statusLabel.Text = "No selected scanner";
        }
    }
}
