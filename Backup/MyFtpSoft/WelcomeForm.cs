using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;


namespace MyFtpSoft
{
    public partial class WelcomeForm : Form
    {
        private Main _main;
        public WelcomeForm(Main main)
        {
            InitializeComponent();
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.BackColor = System.Drawing.Color.LightGray;
            this.TransparencyKey = System.Drawing.Color.LightGray;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            _main = main;
        }
        private void WelcomeForm_Load(object sender, EventArgs e)
        {
            Opacity = 0.0;
            FormComeOut();

            System.Threading.Thread.Sleep(2000);
            FormDispear();
            _main.Enabled = true;
        }

        private void FormComeOut()
        {
            this.Opacity = 0;
            this.Show();
            while (this.Opacity < 1)
            {
                System.Threading.Thread.Sleep(50);
                this.Opacity += 0.05;

            }
        }
        private void FormDispear()
        {
            while (this.Opacity > 0)
            {
                System.Threading.Thread.Sleep(50);
                this.Opacity -= 0.05;
            }
        }
    }
}
