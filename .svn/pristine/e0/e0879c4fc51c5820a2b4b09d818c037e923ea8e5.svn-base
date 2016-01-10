using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MyFtpSoft
{
    public partial class ServerAttrbuteForm : Form
    {
        private string[] fileDetailsArray = null;
        private bool _isFolder;
        public ServerAttrbuteForm(string[] fileDetails,bool isFolder)
        {
            InitializeComponent();
            fileDetailsArray = fileDetails;
            _isFolder = isFolder;
            
        }

        private void ServerAttrbute_Load(object sender, EventArgs e)
        {
            if (_isFolder)
            {
                imageList1.Images.Add(new Icon("../../Icons/Folder.ico"));
            }
            else
            {
                imageList1.Images.Add(Common.ExtractIcon.GetIcon(fileDetailsArray[0], false));
            }
            this.pictureBox1.Image = imageList1.Images[0];
            lblFileName.Text = fileDetailsArray[0];
            lblFileSize.Text = fileDetailsArray[1];
            lblFileModifyTime.Text = fileDetailsArray[2];//5+1+a+s+p+x
            lblLocation.Text = fileDetailsArray[3];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
    }
}
