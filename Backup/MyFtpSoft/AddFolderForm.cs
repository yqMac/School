using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MyFtpSoft
{
    public enum FolderSide
    { 
        Server,
        Local
    }
    public partial class AddFolderForm : Form
    {
        private Main _main;
        private FolderSide _folderSide;
        public AddFolderForm(Main main,FolderSide folderSide)
        {
            InitializeComponent();
            _main = main;
            _folderSide = folderSide;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_folderSide == FolderSide.Local)
            {
                if (textBox1.Text.Trim() != String.Empty)
                {
                    string partUrl = String.Empty;
                    string fullUrl = String.Empty;
                    if (_main.ftpHelper1.Localfolder != "Disk List")
                    {
                        partUrl = _main.ftpHelper1.Localfolder;
                        if (partUrl.EndsWith("\\"))
                        {
                            fullUrl = partUrl + textBox1.Text.Trim(); ;
                        }
                        else
                        {
                            fullUrl = partUrl + "\\" + textBox1.Text.Trim();
                        }
                        Directory.CreateDirectory(fullUrl);

                        _main.FillLocalView(partUrl);
                        this.Close();
                    }
                }
            }
            else if (_folderSide == FolderSide.Server)  
            {
                if (textBox1.Text.Trim() != String.Empty)
                {
                    _main.ftpHelper1.MakeFolder(textBox1.Text.Trim());
                    _main.ftpHelper1.Dir();
                    this.Close();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
