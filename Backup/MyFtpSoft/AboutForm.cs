using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Common;
using System.Xml;

namespace MyFtpSoft
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LoadAmbssilyInfor()
        {
            XmlFiles xmlFileAmbssily = new XmlFiles("UpdateSoft.xml");
            XmlNodeList xmlNodeList = xmlFileAmbssily.GetNodeList("AutoUpdater/Files");

            string[] strArray = new string[2];
            foreach (XmlNode node in xmlNodeList)
            {
                string name =node.Attributes["Name"].Value.Trim();
                string vision = node.Attributes["Ver"].Value.Trim();
                strArray[0] = name;
                strArray[1] = vision;
                listView1.Items.Add(new ListViewItem(strArray,0));
            }
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            LoadAmbssilyInfor();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Comm.CallSysInfoDiaLog();
        }
    }
}
