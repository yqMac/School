using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;


namespace MyFtpSoft
{
    public partial class FtpListView : UserControl
    {
        Main _main;
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        }

        public void SetMain(Main main)
        {
            this._main = main;
        }

        private string folderUrl;

        public string FolderUrl
        {
            get { return folderUrl; }
            set { folderUrl = value; }
        }

        private string stateMessage;

        public string StateMessage
        {
            get { return stateMessage; }
            set { stateMessage = value; }
        }

        public FtpListView()
        {
            InitializeComponent();
        }
        public void AddListItem(ListViewItem item)
        {
            listView1.Items.Add(item);
        }

        public void Clear()
        {
            listView1.Items.Clear();
        }

        public ListView GetListView()
        {
            return listView1;
        }
        public ComboBox GetComboBox()
        {
            return comboBox1;
        }

        public Label GetLable()
        {
            return label1;
        }
        public TreeView GetTreeView()
        {
            return treeView1;
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            if (this.Name == "ftpListViewLocal")
            {
                _main.LocalDoubleClick();
            }
            else if (this.Name == "ftpListViewServer")
            {
                _main.ServerDoublieClick();
            }
        }

        public void SetUrl(string url)
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add(url);
            
            comboBox1.SelectedIndex = 0;
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (this.Name == "ftpListViewLocal")
            {
                _main.FillLocalView(e.Node.Tag.ToString());
                _main.ftpHelper1.Localfolder = e.Node.Tag.ToString();
                SetUrl(e.Node.Tag.ToString());
                
            }
            else if (this.Name == "ftpListViewServer")
            {
                _main.FillServerTree(e.Node);
            }
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (this.Name == "ftpListViewLocal")
            {
                _main.ExpandTreeMethod(e);
            }
        }
    }
}
