using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MyFtpSoft
{
    public partial class RnameForm : Form
    {
        private string _oldFilePath = String.Empty;
        private Main _main;
        public RnameForm(string oldFilePath,Main main)
        {
            InitializeComponent();
            _oldFilePath = oldFilePath;
            this.label2.Text = oldFilePath;
            _main = main;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() != String.Empty)
            {
                _main.ftpHelper1.RnameFile(_oldFilePath, _main.ftpHelper1.Remotefolder + textBox1.Text.Trim());
                _main.ftpHelper1.Dir();
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
