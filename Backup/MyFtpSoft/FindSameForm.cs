using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MyFtpSoft
{
    public enum SameForAll
    { 
        Recover,
        Skip,
        NoSite
    }
    public partial class FindSameForm : Form
    {
        private string _fileName;
        private string _localFileSize;
        private string _localFileDate;
        private string _serverFileSize;
        private string _serverFileDate;
        private SameForAll _sameForAll;

        public FindSameForm(string fileName,string localFileSize,string localDate,
            string serverFileSize,string serverFileDate)
        {
            InitializeComponent();
            this._fileName = fileName;
            this._localFileSize = localFileSize;
            this._localFileDate = localDate;
            this._serverFileSize = serverFileSize;
            this._serverFileDate = serverFileDate;
           
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FindSameForm_Load(object sender, EventArgs e)
        {
            lableFileName.Text = _fileName;
            lableLocalDescrib.Text = "FileSize: " + _localFileSize + "   " + "ModifyDate: " + _localFileDate;
            lableServerDescrib.Text = "FileSize: " + _serverFileSize + "   " + "ModifyDate: " + _serverFileDate;
        }
        //
        //Recover
        //
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
        }
        //
        //Skip
        //
        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
        }

        //
        //All Recover
        //
        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            _sameForAll = SameForAll.Recover;
        }

        //
        //All Skip
        //
        private void button5_Click(object sender,EventArgs e)
        {
            this.DialogResult = DialogResult.No;//5~1-a-s-p-x
            _sameForAll = SameForAll.Skip;
        }
    }
    }

        
