using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Model;
using Common;
using System.Diagnostics;
using MyFTPHelper;


namespace MyFtpSoft
{
    public delegate void DelFtpEvent(MyFTPHelper.FtpEventArgs e);
    public partial class Main : Form
    {
        public bool isExcuteTransfering   = false; //wether the system aready transfering

        private static string currentPath = "disk";
        private string ServerRoot         = string.Empty;
        private bool ServerRootFlag       = false;
        private ProgressBar barFilePass   = null;
        public List<RemoteServers> lsStatueServers;

        private Icon iconFolder           = null;
        private Label labelPrecent        = new Label();
        private string unExceptedMessage  = string.Empty;
        private bool isUnexcept         = false;

        ListView lvLocal;
        Label lableLocal;
        ComboBox comboxLocal;
        TreeView treeViewLoacal;

        ListView lvServer;
        Label lableServer;
        ComboBox comboxServer;
        TreeView treeViewServer;

        RichTextBox rtxtLog;
        ListView lvTransfer;

        private bool signFlag = false;

        public Main()
        {
            InitializeComponent();
            
            ftpHelper1.FilePassProgressCommpleted+=new MyFTPHelper.FtpEventHandler(ftpHelper1_FilePassProgressCommpleted);
            iconFolder = global::MyFtpSoft.Properties.Resources.Folder;
            
        }

        #region InitializeMethod

        private void InitilaProgressBar()
        {
            barFilePass = new ProgressBar();
                 
            barFilePass.Style = ProgressBarStyle.Continuous;
            barFilePass.Width = 100;
          
            barFilePass.ForeColor = Color.Yellow;
            barFilePass.Minimum = 0;
            barFilePass.Maximum = 100;
            barFilePass.Value = 0;
        }

        private void GetControls()
        {
            //get Control
            lvLocal = ftpListViewLocal.GetListView();
            lableLocal = ftpListViewLocal.GetLable();
            comboxLocal = ftpListViewLocal.GetComboBox();
            treeViewLoacal = ftpListViewLocal.GetTreeView();

            lvServer = ftpListViewServer.GetListView();
            lableServer = ftpListViewServer.GetLable();
            comboxServer = ftpListViewServer.GetComboBox();
            treeViewServer = ftpListViewServer.GetTreeView();

            rtxtLog = ftpObserver1.GetRitchTextBox();
            lvTransfer = ftpObserver1.GetListView();

            //Set ImageList for listView
            lvLocal.SmallImageList = imageListLocal;
            lvLocal.LargeImageList = imageListLocal;
            lvServer.SmallImageList = imageListServer;
            lvServer.LargeImageList = imageListServer;
            treeViewLoacal.ImageList = imageListLocatTree;
            treeViewServer.ImageList = imageList2;
            //set ContextMenuStrip
            lvLocal.ContextMenuStrip = contextMenuStripLocal;
            lvServer.ContextMenuStrip = contextMenuStripServer;
           

            ftpListViewLocal.SetMain(this);
            ftpListViewServer.SetMain(this);
        }

        private void InitialStatusBar()
        {
            statusBar1.ShowPanels = true;
            statusBarPanelWelcome.Width = 350;

            SetConnetInfor(global::MyFtpSoft.Properties.Resources.StatueDis, 
                "Welcome: anonymous   State: disConnect");
            statusBarPanelStatus.Width =600;
        }

        public void InitialMyServers()
        {
            HostList.DropDownItems.Clear();
            if (lsStatueServers != null && lsStatueServers.Count > 0)
            {
                ToolStripMenuItem toolStripItemNew;
                foreach (RemoteServers remoServer in lsStatueServers)
                { 
                    toolStripItemNew=new ToolStripMenuItem(remoServer.ServerName,global::MyFtpSoft.Properties.Resources._1);
                    this.HostList.DropDownItems.AddRange(new ToolStripItem[] { toolStripItemNew });
                    toolStripItemNew.Click+=new EventHandler(RemoteServerConnet_Click);
                }
            }
        }

        //
        //Self Add Menu Event
        //
        private void RemoteServerConnet_Click(object sender, EventArgs e)
        {
            ConnectByServerName(sender.ToString());
        }

        #endregion

        #region SamllMethod
        //
        //Swith UpDownLoad
        //
        private void SwitchUpDownLoad(bool bSwitch)
        {
            if (!bSwitch)
            {
                upLoadToolStripMenuItem1.Enabled = false;
                downLoadToolStripMenuItem1.Enabled = false;
                toolStripButton7.Enabled = false;
                toolStripButton8.Enabled = false;
                downLoadToolStripMenuItem.Enabled = false;
                uploadToolStripMenuItem.Enabled = false;
            }
            else
            {
                upLoadToolStripMenuItem1.Enabled = true;
                downLoadToolStripMenuItem1.Enabled = true;
                toolStripButton7.Enabled = true;
                toolStripButton8.Enabled = true;
                downLoadToolStripMenuItem.Enabled = true;
                uploadToolStripMenuItem.Enabled = true;
            }
        }
        //
        //Set the listView Select All Item
        //
        private void ListViewSelectAll(ListView lv)
        {
            for (int i = 0; i < lv.Items.Count; i++)
            {
                lv.Items[i].Selected = true;
            }
        }
        //
        //Set the list Invert Select
        //
        private void ListViewInvertSelect(ListView lv)
        {
            for (int i = 0; i < lv.Items.Count; i++)
            {
                lv.Items[i].Selected = !lv.Items[i].Selected;
            }
        }
        private void SetConnetInfor(Icon icon, string message)
        {
            try
            {
                statusBarPanelWelcome.Text = message;
                statusBarPanelWelcome.Icon = icon;
            }
            catch (Exception)
            { 
            //Ingor
            }
        }
        //
        //judge the fileName wether is in the ServerView
        //
        private ListViewItem JudgeServerExist(string fileName)
        {
            ListViewItem resultItem = null;
            foreach (ListViewItem item in lvServer.Items)
            {
                if (item.Text == fileName)
                {
                    resultItem = item;
                    break;
                }
            }
            return resultItem;
        }
        //
        //Set the Stautus Icon and Message
        //
        private void SetSatusInfor(Icon icon, string message)
        {
            try
            {
                statusBarPanelStatus.Icon = icon;
                statusBarPanelStatus.Text = message;
            }
            catch(Exception)
            {
                //Ignor
            }
        }

        private bool ExistUpLoadAsk(ListViewItem itemServer,ListViewItem itemLocal)
        {
            //get server same file Infor
            string serverFileSize = itemServer.SubItems[1].Text.ToString();
            string serverFileDate = itemServer.SubItems[3].Text.ToString();

            string localFileSize = itemLocal.SubItems[1].Text.ToString();
            string locatFileDate = itemLocal.SubItems[3].Text.ToString();

            FindSameForm findSame = new FindSameForm
                (itemServer.Text, localFileSize, locatFileDate, serverFileSize, serverFileDate);
            findSame.StartPosition = FormStartPosition.CenterScreen;
            if (findSame.ShowDialog() == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //
        //when the download url exist the download file ask to do
        //
        private bool ExistDownLoadAsk(ListViewItem itemServer)
        {
            //get server same file Infor
            string serverFileSize = itemServer.SubItems[1].Text.ToString();
            string serverFileDate = itemServer.SubItems[3].Text.ToString();

            //get locat same fileInfo
            string localFileSize = "0";
            string locatFileDate = String.Empty;
            foreach (ListViewItem item in lvLocal.Items)
            {
                if (item.Text == itemServer.Text)
                {
                    localFileSize = item.SubItems[1].Text.ToString();
                    locatFileDate = item.SubItems[3].Text.ToString();
                }
            }

            FindSameForm findSame = new FindSameForm
                (itemServer.Text, localFileSize, locatFileDate, serverFileSize, serverFileDate);
            findSame.StartPosition = FormStartPosition.CenterScreen;
            if (findSame.ShowDialog() == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //
        //Set MyServerControls tool
        //
        private void ConnectByServerName(string serverName)
        {
            foreach (RemoteServers remoteServer in lsStatueServers)
            {
                if (remoteServer.ServerName == serverName)
                {
                    cbxServer.Text = remoteServer.ServerIP;
                    txtProt.Text = remoteServer.ServerPort.ToString();
                    if (remoteServer.IsAnonymous == 1)
                    {
                        CheckBoxAnonymous.Checked = true;
                    }
                    else if (remoteServer.IsAnonymous == 0)
                    {
                        CheckBoxAnonymous.Checked = false;
                    }
                    txtUserName.Text = remoteServer.UserID;
                    txtUserPwd.Text = remoteServer.UserPwd;
                }
            }
        }
        //
        //ListView Sepearte
        //
        private void ListViewStye(ListView lv)
        {
            foreach (ListViewItem item in lv.Items)
            {
                if (item.Index % 2 == 0)
                {
                    item.BackColor = Color.FromArgb(243, 245, 247);
                }
            }
        }
        //
        //Set Anonymous
        //
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxAnonymous.Checked)
            {
                txtUserName.Text = "anonymous";
                txtUserPwd.Text = "anonymous";
                txtUserName.ReadOnly = true;
                txtUserPwd.ReadOnly = true;
            }
            else
            {
                txtUserName.ReadOnly = false;
                txtUserPwd.ReadOnly = false;
            }
        }
        //
        //is folder
        //
        private bool IsFolder(string dirName)
        {
            FileInfo fileInfo = new FileInfo(dirName);
            if ((fileInfo.Attributes & FileAttributes.Directory) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //
        //add the Progress to the StatueBar
        //
        private void statusBar1_DrawItem(object sender, StatusBarDrawItemEventArgs sbdevent)
        {
            barFilePass.Parent = this.statusBar1;
            labelPrecent.Parent = this.statusBar1;
            labelPrecent.BringToFront();
            labelPrecent.AutoSize = true;
            labelPrecent.BackColor = Color.Yellow;

            barFilePass.SetBounds(statusBarPanelWelcome.Width, 2, statusBarPanelFileProgress.Width, this.statusBar1.ClientRectangle.Height - 2);
            labelPrecent.SetBounds(statusBarPanelWelcome.Width + statusBarPanelFileProgress.Width/3, 5, 40, this.statusBar1.ClientRectangle.Height - 3);
        }
        //
        //Display Msg 
        //
        private void AddMessageToObserve(Color showColor, string message)
        {
            rtxtLog.SelectionColor = showColor;
            if (message != String.Empty)
            {
                rtxtLog.AppendText(message);
            }
            rtxtLog.AppendText(Environment.NewLine);
            rtxtLog.SelectionStart = rtxtLog.TextLength;
            rtxtLog.ScrollToCaret();
        }

        //
        //ChangeSign
        //
        [DebuggerStepThroughAttribute()]
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!signFlag)
            {
                statusBarPanelSign.Icon = global::MyFtpSoft.Properties.Resources.Red;
                signFlag = true;
            }
            else
            {
                statusBarPanelSign.Icon = global::MyFtpSoft.Properties.Resources.Green;
                signFlag = false;
            }
        }

        #endregion

        #region SelfOperate

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
        //
        //Refresh
        //
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            if (lvLocal.Focused)
            {
                FillLocalView(ftpHelper1.Localfolder);
            }
            else if (lvServer.Focused)
            {
                ftpHelper1.Dir();
            }
        }
        //
        //abor
        //
        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            ftpHelper1.Abor();
        }
        //
        //Delete File
        //
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < lvLocal.SelectedItems.Count; i++)
                {
                    if (lvLocal.SelectedItems[i].Text == "...")
                    {
                        MessageBox.Show("Invalid operate!", "invalid",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        ListViewItem item = lvLocal.SelectedItems[i];
                        if (MessageBox.Show("Delete file or folder\"" + item.Text + "\"?",
                            "Delete Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Information)
                             == DialogResult.Yes)
                        {
                            string fileFullName = item.Tag.ToString();
                            if (IsFolder(fileFullName))
                                Directory.Delete(fileFullName);
                            else
                                File.Delete(fileFullName);
                        }
                    }
                }
                FillLocalView(ftpHelper1.Localfolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Delete Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        //
        //Add File To Transfer Queue
        //
        private void AddFileToTransferQueue(ListViewItem lvItem, int flag)
        {
            string name = String.Empty;
            string fileSize = String.Empty;
            string target = String.Empty;
            if (flag == 0)//down
            {
                name = lvItem.Text.ToString();
                fileSize = lvItem.SubItems[1].Text.ToString();
                target = ftpHelper1.Localfolder;
            }
            else //Upload
            {
                name = lvItem.Text.ToString();
                fileSize = lvItem.SubItems[1].Text.ToString();
                target = ftpHelper1.Remotefolder;
            }

            string[] items = new string[3];
            items[0] = name;
            items[1] = fileSize;
            items[2] = target;
            ListViewItem item = new ListViewItem(items, flag);
            lvTransfer.Items.Add(item);
        }
        //
        //DownLoad
        //
        private void downLoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ftpHelper1.Localfolder == "Disk List")
            {
                MessageBox.Show("Url set error! Set a invadite url to save files ", "URL ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; i < lvServer.SelectedItems.Count; i++)
            {
                ListViewItem lvItemSelect = lvServer.SelectedItems[i];
                //is file
                if (lvItemSelect.Tag.ToString() == "0")
                {
                    timer1.Enabled = true;
                    string fileName = lvItemSelect.Text;
                    //Exist File
                    if (File.Exists(ftpHelper1.Localfolder + "\\" + fileName))
                    {
                        if (!ExistDownLoadAsk(lvItemSelect))
                        {
                            timer1.Enabled = false;
                            continue;
                        }
                    }
                    //add to the Transfer Queue
                    AddFileToTransferQueue(lvItemSelect, 0);

                }
            }
            if (!isExcuteTransfering)// not transfering the do it
            {
                isExcuteTransfering = true;
                ExcuteTranfer();
            }

        }
        //
        //Upload 
        //
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lvLocal.SelectedItems.Count; i++)
            {
               
                ListViewItem selectItem = lvLocal.SelectedItems[i];
                if (!IsFolder(selectItem.Tag.ToString()))
                {
                    timer1.Enabled = true;
                    ListViewItem itemServer = JudgeServerExist(selectItem.Text);
                    if ((itemServer) != null)
                    {
                        if (!ExistUpLoadAsk(itemServer, selectItem))
                        {
                            timer1.Enabled = false;
                            continue;
                        }
                    }
                    AddFileToTransferQueue(selectItem, 1);
                }
            }
            if (!isExcuteTransfering)// not transfering the do it
            {
                isExcuteTransfering = true;
                ExcuteTranfer();
            }
        }
        //
        //Excute The Transfer Queue
        //
        private void ExcuteTranfer()
        {
            if (lvTransfer.Items.Count > 0)
            {
                ListViewItem item = lvTransfer.Items[0];
                string fileName = item.Text.ToString();
                string sFileSize = item.SubItems[1].Text.ToString();
                int ifileSize = Convert.ToInt32(sFileSize.Substring(0, sFileSize.Length - 2));

                this.Cursor = Cursors.WaitCursor;
                if (item.ImageIndex == 0) //begin downLoad
                {
                    SetSatusInfor(global::MyFtpSoft.Properties.Resources.transferDown1, "[OPERATE]   Start download!  ...");
                    ftpHelper1.DowLoadFile(fileName, ifileSize);
                }
                else if (item.ImageIndex == 1)//begion upLoad
                {
                    SetSatusInfor(global::MyFtpSoft.Properties.Resources.transferUp1, "[OPERATE]   Start upload!  ...");
                    ftpHelper1.UpLoadFile(fileName, ifileSize);
                }

            }
        }
        //
        //SaveStatus
        //
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            SerializeClass.SerializeObject(lsStatueServers);
        }
        //
        //Clear the Queue
        //
        private void clearQueueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvTransfer.Items.Clear();
        }

        //
        //show queue infor
        //
        private void queueInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int countUp = 0;
            int countDown = 0;
            for (int i = 0; i < lvTransfer.Items.Count; i++)
            {
                if (lvTransfer.Items[i].ImageIndex == 1)
                {
                    countUp++;
                }
                else if (lvTransfer.Items[i].ImageIndex == 0)
                {
                    countDown++;
                }
            }
            QueueInformationForm qf = new QueueInformationForm(lvTransfer.Items.Count, countUp, countDown);
            qf.Show();
        }

        //
        //Show file Properties
        //
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (lvLocal.Focused)
            {
                string filePath = lvLocal.SelectedItems[0].Tag.ToString();
                Comm.ShowFileProperties(filePath);
            }
            else if (lvServer.Focused)
            {
                //not finish
            }
        }
        //
        //add in the transfer queue
        //
        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            if (lvLocal.Focused)
            {
                for (int i = 0; i < lvLocal.SelectedItems.Count; i++)
                {

                    ListViewItem selectItem = lvLocal.SelectedItems[i];
                    if (!IsFolder(selectItem.Tag.ToString()))
                    {
                        timer1.Enabled = true;
                        ListViewItem itemServer = JudgeServerExist(selectItem.Text);
                        if ((itemServer) != null)
                        {
                            if (!ExistUpLoadAsk(itemServer, selectItem))
                            {
                                timer1.Enabled = false;
                                continue;
                            }
                        }
                        AddFileToTransferQueue(selectItem, 1);
                    }
                }
            }
            else if (lvServer.Focused)
            {
                if (ftpHelper1.Localfolder == "Disk List")
                {
                    MessageBox.Show("Url set error! Set a invadite url to save files ", "URL ERROR",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                for (int i = 0; i < lvServer.SelectedItems.Count; i++)
                {
                    ListViewItem lvItemSelect = lvServer.SelectedItems[i];
                    //is file
                    if (lvItemSelect.Tag.ToString() == "0")
                    {
                        timer1.Enabled = true;
                        string fileName = lvItemSelect.Text;
                        //Exist File
                        if (File.Exists(ftpHelper1.Localfolder + "\\" + fileName))
                        {
                            if (!ExistDownLoadAsk(lvItemSelect))
                            {
                                timer1.Enabled = false;
                                continue;
                            }
                        }
                        //add to the Transfer Queue
                        AddFileToTransferQueue(lvItemSelect, 0);
                    }
                }
            }
        }
        //
        //Connect
        //
        private void Connect()
        {
            try
            {
                if (cbxServer.Text.Trim().Length > 0 &&
                    txtUserName.Text.Trim().Length > 0 &&
                    txtUserPwd.Text.Trim().Length > 0)
                {
                    ftpHelper1.Username = txtUserName.Text.Trim();
                    ftpHelper1.Password = txtUserPwd.Text.Trim();
                    ftpHelper1.Port = Convert.ToInt32(txtProt.Text.Trim());
                    ftpHelper1.Hostname = cbxServer.Text.Trim();
                    ftpHelper1.Connect();
                }
                else
                {
                    MessageBox.Show("Unvalidate input", "Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //
        //local DoubleClick
        //
        public void LocalDoubleClick()
        {
            if (lvLocal.SelectedItems[0].Tag.ToString() == "...")
            {
                DirectoryInfo di = new DirectoryInfo(currentPath);
                if (di.Parent != null)
                {
                    string parentUrl = di.Parent.FullName;
                    FillLocalView(parentUrl);
                    currentPath = parentUrl;
                    ftpListViewLocal.SetUrl(currentPath);
                    ftpHelper1.Localfolder = currentPath;
                }
                else
                {
                    InitializeLocalView();
                }
            }
            else
            {
                currentPath = lvLocal.SelectedItems[0].Tag.ToString();
                if (IsFolder(currentPath))
                {
                    FillLocalView(currentPath);
                    ftpListViewLocal.SetUrl(currentPath);
                    ftpHelper1.Localfolder = currentPath;
                }
            }

        }
        //
        //Server DoubleClick
        //
        public void ServerDoublieClick()
        {
            if (lvTransfer.Items.Count > 0)
            {

            }
            else 
            {
                SetSatusInfor(global::MyFtpSoft.Properties.Resources.Wait,
                     "[OPERATE]   Getting DirList! ...");
                timer1.Enabled = true;
                if (lvServer.SelectedItems[0].Text == "...")
                {
                    this.Cursor = Cursors.WaitCursor;
                    ftpHelper1.DirUp();
                    ftpHelper1.Dir();
                }
                else
                {
                    string dirName = lvServer.SelectedItems[0].Text;
                    string isFolder = lvServer.SelectedItems[0].Tag.ToString();
                    if (isFolder == "1")
                    {
                        this.Cursor = Cursors.WaitCursor;
                        ftpHelper1.DirChange(dirName);
                        ftpHelper1.Dir();
                    }
                }
            }
        }
        //
        //Server Delte File
        //
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lvServer.SelectedItems.Count; i++)
            {
                if (lvServer.SelectedItems[0].Text == "...")
                {
                    MessageBox.Show("Invalid operate!", "invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (MessageBox.Show("Delete file or folder\"" + lvServer.SelectedItems[i].Text.ToString() + "\"?",
                        "Delete Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        //is file
                        if (lvServer.SelectedItems[i].Tag.ToString() == "0")
                        {
                            string fileName = lvServer.SelectedItems[i].Text;
                            ftpHelper1.DeleteFile(fileName);
                        }
                        //is folder
                        else
                        {
                            string folderName = lvServer.SelectedItems[0].Text;
                            ftpHelper1.DeleteFolder(ftpHelper1.Remotefolder + folderName);
                        }
                    }
                }
            }
            ftpHelper1.Dir();
        }
        //
        //DownLoad to deskTop
        //
        private void deskTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string deskTopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ftpHelper1.Localfolder = deskTopPath;
            ftpListViewLocal.SetUrl(deskTopPath);
            FillLocalView(deskTopPath);

            for (int i = 0; i < lvServer.SelectedItems.Count; i++)
            {
                ListViewItem lvItemSelect = lvServer.SelectedItems[i];
                //is file
                if (lvItemSelect.Tag.ToString() == "0")
                {
                    timer1.Enabled = true;
                    string fileName = lvItemSelect.Text;
                    //Exist File
                    if (File.Exists(ftpHelper1.Localfolder + "\\" + fileName))
                    {
                        if (!ExistDownLoadAsk(lvItemSelect))
                        {
                            timer1.Enabled = false;
                            continue;
                        }
                    }
                    //add to the Transfer Queue
                    AddFileToTransferQueue(lvItemSelect, 0);

                }
            }
            if (!isExcuteTransfering)// not transfering the do it
            {
                isExcuteTransfering = true;
                ExcuteTranfer();
            }
        }

        //
        //DownLoad to A appointed folder
        //
        private void browserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;
            folderBrowserDialog1.ShowNewFolderButton = true;
            folderBrowserDialog1.Description = "Select a folder to save download files";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string selectPath = folderBrowserDialog1.SelectedPath;
                ftpHelper1.Localfolder = selectPath;
                ftpListViewLocal.SetUrl(selectPath);

                FillLocalView(selectPath);
                for (int i = 0; i < lvServer.SelectedItems.Count; i++)
                {
                    ListViewItem lvItemSelect = lvServer.SelectedItems[i];
                    //is file
                    if (lvItemSelect.Tag.ToString() == "0")
                    {
                        timer1.Enabled = true;
                        string fileName = lvItemSelect.Text;
                        //Exist File
                        if (File.Exists(ftpHelper1.Localfolder + "\\" + fileName))
                        {
                            if (!ExistDownLoadAsk(lvItemSelect))
                            {
                                timer1.Enabled = false;
                                continue;
                            }
                        }
                        //add to the Transfer Queue
                        AddFileToTransferQueue(lvItemSelect, 0);

                    }
                }
                if (!isExcuteTransfering)// not transfering the do it
                {
                    isExcuteTransfering = true;
                    ExcuteTranfer();
                }
            }
        }
        //
        //Server new folder
        //
        private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddFolderForm addFolderForm = new AddFolderForm(this, FolderSide.Server);
            addFolderForm.Show();
        }
        //
        //Send wait single
        //
        private void timerWait_Tick(object sender, EventArgs e)
        {
            ftpHelper1.WaitSingle();
        }
        //
        //server Rname
        //
        private void rnameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = lvServer.SelectedItems[0].Text;
            string oldeFileUrl = ftpHelper1.Remotefolder + fileName;
            RnameForm rnameForm = new RnameForm(oldeFileUrl, this);
            rnameForm.StartPosition = FormStartPosition.CenterScreen;
            rnameForm.Show();

        }
        //
        //Server file Attrbute
        //
        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem item = lvServer.SelectedItems[0];
            string fileName = item.Text;
            string fileSize = item.SubItems[1].Text;
            string fileMotify = item.SubItems[3].Text;
            string fileLoction = ftpHelper1.Remotefolder;

            string[] fileDetails = new string[4];
            fileDetails[0] = fileName;
            fileDetails[2] = fileMotify;
            fileDetails[3] = fileLoction;
            ServerAttrbuteForm serverAttrbute;
            if (item.Tag.ToString() == "1")
            {
                fileDetails[1] = "Unkown";
                serverAttrbute = new ServerAttrbuteForm(fileDetails, true);
            }
            else
            {
                fileDetails[1] = fileSize;
                serverAttrbute = new ServerAttrbuteForm(fileDetails, false);
            }
            serverAttrbute.Show();
        }

        #endregion

        #region load local File System

        public void ExpandTreeMethod(TreeViewEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            treeViewLoacal.BeginUpdate();

            TreeNode selectNode = e.Node;
            string[] driverList = null;
            selectNode.Nodes.Clear();
            driverList = Directory.GetDirectories(selectNode.Tag.ToString());
            for (int i = 0; i < driverList.Length; i++)
            {
                string firstGetName = driverList[i].Substring(driverList[i].LastIndexOf('\\') + 1);
                TreeNode newFirstNode = new TreeNode(firstGetName, 0, 0);
                newFirstNode.Tag = driverList[i];
                selectNode.Nodes.Add(newFirstNode);
                string[] driverListSenco = null;
                try
                {
                    driverListSenco = Directory.GetDirectories(newFirstNode.Tag.ToString());
                    if (driverListSenco != null)
                    {
                        for (int j = 0; j < driverListSenco.Length; j++)
                        {
                            string secoName = driverListSenco[j].Substring(driverListSenco[j].LastIndexOf("\\") + 1);
                            TreeNode newSecoNode = new TreeNode(secoName, 0, 0);
                            newSecoNode.Tag = driverListSenco[j];
                            newFirstNode.Nodes.Add(newSecoNode);
                        }
                    }
                }
                catch 
                {}
            }
            treeViewLoacal.EndUpdate();
            this.Cursor = Cursors.Default;
        }
        //
        //Initial Tree
        //
        private void InitializeLoacalTree()
        {
            int idImage = 1;
            string[] driverList = Directory.GetLogicalDrives();
            for (int i = 0; i < driverList.Length; i++)
            {
                string diskName = string.Empty;
                diskName = "local Disk(" + driverList[i].Substring
                    (0, driverList[i].Length - 1) + ")";
                Icon ico = ExtractIcon.GetIcon(driverList[i], true);
                imageListLocatTree.Images.Add(ico);
                int index = idImage++;
                TreeNode node = new TreeNode(diskName, index, index);
                node.Tag = driverList[i];
                treeViewLoacal.Nodes.Add(node);


                DriveInfo dry = new DriveInfo(driverList[i]);
                if (dry.IsReady)
                {
                    string[] dirList = Directory.GetDirectories(node.Tag.ToString());
                    for (int j = 0; j < dirList.Length; j++)
                    {
                        string dirName = dirList[j].Substring(dirList[j].LastIndexOf('\\'));
                        TreeNode newNode = new TreeNode(dirName, 0, 0);
                        newNode.Tag = dirList[j];

                        node.Nodes.Add(newNode);
                    }
                }
            }
        }
        //
        //Initial ListView
        //
        private void InitializeLocalView()
        {
            this.Cursor = Cursors.WaitCursor;
            lvLocal.BeginUpdate();
            lvLocal.Items.Clear();
            imageListLocal.Images.Clear();

            string[] driverList = Directory.GetLogicalDrives();
            ListViewItem item;
            int idImage = 0;

            string[] diskInfor;
            for (int i = 0; i < driverList.Length; i++)
            {
                diskInfor = new string[4];
                diskInfor[0] = "local Disk(" + driverList[i].Substring
                    (0, driverList[i].Length - 1) + ")";

                DirectoryInfo dr = new DirectoryInfo(driverList[i]);
                string modiflyDate = dr.LastWriteTime.ToString();

                diskInfor[1] = String.Empty;
                diskInfor[2] = String.Empty;
                diskInfor[3] = modiflyDate;

                DriveInfo dry = new DriveInfo(driverList[i]);
                if (dry.IsReady)
                {
                    diskInfor[1] = (((dry.TotalSize / 1024) / 1024) / 1024).ToString() + "GB";
                    diskInfor[2] = dry.DriveFormat.ToString();
                }

                Icon ico = ExtractIcon.GetIcon(driverList[i], true);
                imageListLocal.Images.Add(ico);

                item = new ListViewItem(diskInfor, idImage++);
                item.Tag = driverList[i];
                lvLocal.Items.Add(item);
            }
            lvLocal.EndUpdate();
            this.Refresh();
            this.Cursor = Cursors.Default;
            ftpListViewLocal.SetUrl("Disk List");
            ftpHelper1.Localfolder = "Disk List";
            lableLocal.Text = driverList.Length + " Disk(s) , 0 File(s) , 0 Directories  [*.*]";

            ListViewStye(lvLocal);
        }

        public void FillLocalView(string path)
        {
            try
            {
                if (path == "Disk List")
                {
                    InitializeLocalView();
                }
                else
                {
                    this.Cursor = Cursors.WaitCursor;
                    lvLocal.BeginUpdate();
                    lvLocal.Items.Clear();
                    imageListLocal.Images.Clear();
                    int idImage = 0;
                    Icon icoUp = global::MyFtpSoft.Properties.Resources.up2;
                    imageListLocal.Images.Add(icoUp);
                 
                    ListViewItem lvItemUp = new ListViewItem("...", idImage++);
                    lvItemUp.Tag = "...";
                    lvLocal.Items.Add(lvItemUp);

                    DirectoryInfo directory = new DirectoryInfo(path);
                    DirectoryInfo[] directoryList = directory.GetDirectories();
                    FileInfo[] fileList = directory.GetFiles();


                    ListViewItem item;
                    #region add Folder
                    foreach (DirectoryInfo direInfo in directoryList)
                    {
                        string[] direArry = new string[4];
                        direArry[0] = direInfo.FullName.Substring
                            (direInfo.FullName.LastIndexOf('\\') + 1);
                        direArry[1] = String.Empty;
                        direArry[2] = direInfo.Extension;
                        direArry[3] = direInfo.LastWriteTime.ToString();

                        Icon ico = ExtractIcon.GetIcon(direInfo.FullName, true);
                        imageListLocal.Images.Add(ico);
                       
                        item = new ListViewItem(direArry, idImage++);
                        item.Tag = direInfo.FullName;
                        lvLocal.Items.Add(item);
                    }
                    #endregion

                    #region addFile
                    foreach (FileInfo fileInfo in fileList)
                    {
                        string[] fileArry = new string[4];
                        fileArry[0] = fileInfo.FullName.Substring
                            (fileInfo.FullName.LastIndexOf('\\') + 1);
                        fileArry[1] = (fileInfo.Length).ToString() + "KB";
                        fileArry[2] = fileInfo.Extension;
                        fileArry[3] = fileInfo.LastWriteTime.ToString();

                        Icon ico = ExtractIcon.GetIcon(fileInfo.FullName, true);
                        imageListLocal.Images.Add(ico);
                        
                        item = new ListViewItem(fileArry, idImage++);
                        item.Tag = fileInfo.FullName;
                        lvLocal.Items.Add(item);
                    }
                    #endregion

                    ListViewStye(lvLocal);///
                    lvLocal.EndUpdate();
                    this.Refresh();
                    this.Cursor = Cursors.Default;

                    lableLocal.Text = "0 Disk(s) , " + fileList.Length + " File(s) , " + directoryList.Length + " Directories  [*.*]";
                }
            }
            catch (Exception)
            {
                lvLocal.EndUpdate();
                this.Refresh();
                this.Cursor = Cursors.Default;
            }

        }

     

        #endregion
      
        #region FtpEvent

        private void ftpHelper1_ResponseUnexpected(object sender, FtpEventArgs e)
        {
            DelFtpEvent delFtpEvent = new DelFtpEvent(ResponseUnexpected);
            this.Invoke(delFtpEvent, e);
        }
        private void ResponseUnexpected(FtpEventArgs e)
        {
            string code = e.SMessage.Substring(0, 3);
            try
            {
                int iCode = Convert.ToInt32(code);
                int index = e.SMessage.LastIndexOf("\n");
                unExceptedMessage = e.SMessage.Substring(index + 1);
                isUnexcept = true;

                AddMessageToObserve(Color.DarkOrange, e.SMessage.Substring(0, index));
            }
            catch
            {
                return;
            }
        }


        private void ftpHelper1_FilePassProgressCommpleted(object sender, MyFTPHelper.FtpEventArgs e)
        {
            DelFtpEvent delFtpEvent = new DelFtpEvent(FilePassProgressCommpleted);
            this.Invoke(delFtpEvent, e);
        }
        private void FilePassProgressCommpleted(FtpEventArgs e)
        {
            int precent = (int)(((float)e.AreadyBytes / (float)e.Totalbytes) * 100);
            barFilePass.Value = precent;
            labelPrecent.Text = precent.ToString()+"% Finished";
            statusBarPanelStatus.Text = "Transfer:" + e.SMessage + " Time: " + ((float)e.Timeelapsed / 1000).ToString()+ " (s)  ";
        }

        private void ftpHelper1_FileUpLoadCommpleted(object sender, MyFTPHelper.FtpEventArgs e)
        {
            DelFtpEvent delFtpEvent = new DelFtpEvent(FileUpLoadCommpleted);
            this.Invoke(delFtpEvent, e);
        }
        private void FileUpLoadCommpleted(FtpEventArgs e)
        {
            string msg = "FileUpload Transfered " + e.Totalbytes.ToString() + " bytes in " +
                ((float)e.Timeelapsed / 1000).ToString() + " seconds" + "\r\n";
            AddMessageToObserve(Color.Black, msg);
            ftpHelper1.Dir();
            this.Cursor = Cursors.Default;
            timer1.Enabled = false;
            lvTransfer.Items.RemoveAt(0);
            if (lvTransfer.Items.Count > 0)
            {
                ExcuteTranfer();
            }
            else 
            {
                isExcuteTransfering = false;
                SetSatusInfor(global::MyFtpSoft.Properties.Resources.list, "[OPERATE]   UpLoad Commpleted,FileList! ");
            }
        }

        private void ftpHelper1_FileDownLoadCommpleted(object sender, MyFTPHelper.FtpEventArgs e)
        {
            DelFtpEvent delFtpEvent = new DelFtpEvent(FileDownLoadCommpleted);
            this.Invoke(delFtpEvent, e);
        }
        private void FileDownLoadCommpleted(FtpEventArgs e)
        {
            string msg = "FileDownload Transfered " + e.Totalbytes.ToString() + " bytes in " +
                ((float)e.Timeelapsed / 1000).ToString() + " seconds" + "\r\n";
            AddMessageToObserve(Color.Black, msg);
            FillLocalView(ftpHelper1.Localfolder);
            this.Cursor = Cursors.Default;
            timer1.Enabled = false;
            lvTransfer.Items.RemoveAt(0);
            if (lvTransfer.Items.Count > 0)
            {
                ExcuteTranfer();
            }
            else
            {
                isExcuteTransfering = false;
                SetSatusInfor(global::MyFtpSoft.Properties.Resources.list, "[OPERATE]   DownLoad Commpleted,FileList! ");
            }
        }

        private void ftpHelper1_ConnectCommpleted(object sender, MyFTPHelper.FtpEventArgs e)
        {
            DelFtpEvent delFtpEvent = new DelFtpEvent(ConnectCommpleted);
            this.Invoke(delFtpEvent, e);
        }
        private void ConnectCommpleted(FtpEventArgs e)
        {
            #region Set StateBar
            SetConnetInfor(global::MyFtpSoft.Properties.Resources.StateCon, "Welcome: anonymous   State: Connect");
            SetSatusInfor(global::MyFtpSoft.Properties.Resources.login, "Now logning! ...");
            #endregion
            SwitchUpDownLoad(true);
            ftpHelper1.Login();
            timerWait.Enabled = true;
        }

        private void ftpHelper1_Message(object sender, MyFTPHelper.FtpEventArgs e)
        {
            DelFtpEvent delFtpEvent = new DelFtpEvent(MessageObserve);
            this.Invoke(delFtpEvent, e);
        }
        private void MessageObserve(FtpEventArgs e)
        {
            AddMessageToObserve(Color.Green, e.SMessage);
        }

        private void ftpHelper1_Error(object sender, MyFTPHelper.FtpEventArgs e)
        {
            DelFtpEvent delFtpEvent = new DelFtpEvent(ErrorObserver);
            this.Invoke(delFtpEvent, e);
        }
        private void ErrorObserver(FtpEventArgs e)
        {
            AddMessageToObserve(Color.Red, e.Function + "-" + e.SMessage);
        }

        private void ftpHelper1_CommandCommpleted(object sender, MyFTPHelper.FtpEventArgs e)
        {
            DelFtpEvent delFtpEvent = new DelFtpEvent(CommandCommpleted);
            this.Invoke(delFtpEvent, e);
        }
        private void CommandCommpleted(FtpEventArgs e)
        {
            AddMessageToObserve(Color.Blue, e.SMessage);
        }

        private void ftpHelper1_BaseInfor(object sender, MyFTPHelper.FtpEventArgs e)
        {
            DelFtpEvent delFtpEvent = new DelFtpEvent(BaseInfor);
            this.Invoke(delFtpEvent, e);
        }
        private void BaseInfor(FtpEventArgs e)
        {
            AddMessageToObserve(Color.Black, e.SMessage);
        }

        private void ftpHelper1_LoginCommpleted(object sender, MyFTPHelper.FtpEventArgs e)
        {
            DelFtpEvent delFtpEvent = new DelFtpEvent(LoginCommpleted);
            this.Invoke(delFtpEvent, e);
        }
        private void LoginCommpleted(FtpEventArgs e)
        {
            SetSatusInfor(global::MyFtpSoft.Properties.Resources.Wait, "[OPERATE]   Getting DirList! ...");
            ftpHelper1.Dir();
        }

        private void ftpHelper1_DirCommpleted(object sender, MyFTPHelper.FtpEventArgs e)
        {
            DelFtpEvent delFtpEvent = new DelFtpEvent(DirCommpleted);
            this.Invoke(delFtpEvent, e);
        }
        private void DirCommpleted(FtpEventArgs e)
        {
            try
            {
                int fileCount = 0;
                int directoryCount = 0;
                int i = 0;
                int imageIndex = 0;
                string isFolder = "0";

                #region Add TransferMsg
                string msg = "\r\nTransfered " + e.Totalbytes.ToString() + " bytes in " +
                    ((float)e.Timeelapsed / 1000).ToString() + " seconds" + "\r\n";
                AddMessageToObserve(Color.Black, msg);
                #endregion

                if (!ServerRootFlag)
                {
                    ServerRoot = ftpHelper1.Remotefolder;
                    ServerRootFlag = true;
                    InitializeServerTree(ServerRoot);
                }


                lvServer.BeginUpdate();
                lvServer.Items.Clear();
                imageListServer.Images.Clear();

                #region Add Back
                if (ftpHelper1.Remotefolder != ServerRoot)
                {
                    Icon icoUp = global::MyFtpSoft.Properties.Resources.up2;
                    imageListServer.Images.Add(icoUp);
                    ListViewItem lvItemUp = new ListViewItem("...", imageIndex++);
                    lvItemUp.Tag = "...";
                    lvServer.Items.Add(lvItemUp);

                    //FillServerTree();
                }
                #endregion

                #region Add Folder and Files
                for (i = 0; i < ftpHelper1.listServerData.Count; i++)
                {
                    string[] items = new string[4];
                    items[0] = ftpHelper1.listServerData[i].fileName;
                    items[3] = ftpHelper1.listServerData[i].date;
                    if (ftpHelper1.listServerData[i].isDirectory)
                    {
                        imageListServer.Images.Add(iconFolder);
                        items[1] = String.Empty;
                        items[2] = String.Empty;
                        directoryCount++;
                        isFolder = "1";
                    }
                    else
                    {
                        imageListServer.Images.Add(ExtractIcon.GetIcon(items[0], false));
                        items[1] = ftpHelper1.listServerData[i].size.ToString() + "KB";
                        if (items[0].Contains("."))
                        {
                            items[2] = items[0].Substring(items[0].LastIndexOf('.'),
                                       items[0].Length - items[0].LastIndexOf('.'));
                        }
                        fileCount++;
                        isFolder = "0";
                    }
                    ListViewItem lvItem = new ListViewItem(items, imageIndex++);
                    lvItem.Tag = isFolder;
                    lvServer.Items.Add(lvItem);
                }
                #endregion

                #region add others
                ftpListViewServer.SetUrl(ftpHelper1.Remotefolder);
                lableServer.Text = "0 Disk(s) , " + fileCount + " File(s) , " + directoryCount +
                                 " Directories  " + e.Totalbytes.ToString() + " bytes offer [*.*]";
                ListViewStye(lvServer);
                timer1.Enabled = false;
                statusBarPanelSign.Icon = global::MyFtpSoft.Properties.Resources.Gray;
                #endregion

                lvServer.EndUpdate();
                if (!isUnexcept)//Sucessfully
                {
                    SetSatusInfor(global::MyFtpSoft.Properties.Resources.list, "[OPERATE]   DirList Ready! ...");
                }
                else //Failure  unexcepted
                {
                    SetSatusInfor(global::MyFtpSoft.Properties.Resources.Error, unExceptedMessage);
                    isUnexcept = false;
                }
                this.Cursor = Cursors.Default;
                ftpHelper1.listServerData.Clear();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void InitializeServerTree(string root)
        {
            //treeViewServer.Nodes.Clear();
            TreeNode nodeRoot = new TreeNode(root, 0, 0);
            nodeRoot.Tag = root;
            treeViewServer.Nodes.Add(nodeRoot);
            for (int i = 0; i < ftpHelper1.listServerData.Count; i++)
            {
                TreeNode newNode = null;
                if (ftpHelper1.listServerData[i].isDirectory)
                {
                    newNode = new TreeNode(ftpHelper1.listServerData[i].fileName, 1, 1);
                    newNode.Tag = root + ftpHelper1.listServerData[i].fileName;
                    nodeRoot.Nodes.Add(newNode);
                }
            }
        }

        private void FillServerTree()
        {
            treeViewServer.SelectedNode.Nodes.Clear();
            treeViewServer.SelectedNode.ImageIndex = 0;
            treeViewServer.SelectedNode.SelectedImageIndex = 0;
            for (int i = 0; i < ftpHelper1.listServerData.Count; i++)
            {
                {
                    TreeNode newNode = null;
                    if (ftpHelper1.listServerData[i].isDirectory)
                    {
                        newNode = new TreeNode(ftpHelper1.listServerData[i].fileName, 1, 1);
                        if (comboxServer.SelectedItem.ToString().EndsWith("/"))
                        {
                            newNode.Tag = comboxServer.SelectedItem.ToString() + ftpHelper1.listServerData[i].fileName;
                        }
                        else
                        {
                            newNode.Tag = comboxServer.SelectedItem.ToString() + "/" + ftpHelper1.listServerData[i].fileName;
                        }
                        treeViewServer.SelectedNode.Nodes.Add(newNode);
                    }
                }
            }
        }
        public void FillServerTree(TreeNode node)
        {
            string dirName = node.Tag.ToString();
            this.Cursor = Cursors.WaitCursor;
            ftpHelper1.DirChange(dirName);
            ftpHelper1.Dir();
        }

        #endregion

        #region ToolEvent

        //
        //ShowMyServerControl tool
        //
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            ServerControlForm serverControl = new ServerControlForm(this);
            serverControl.Show();
        }

        //
        //DisConnect tool
        //
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            SwitchUpDownLoad(false);
            timer1.Enabled = false;
            ServerRootFlag = false;
            toolStripButtonDisConnect.Enabled = false;
            toolStripButtonConnect.Enabled = true;
            connectToolStripMenuItem.Enabled = true;
            disConnnectToolStripMenuItem.Enabled =false;
            lvServer.Items.Clear();
            ftpHelper1.DisConnect();
            ftpHelper1.ClearCommandList();
            timerWait.Enabled = false;
            SetConnetInfor(global::MyFtpSoft.Properties.Resources.StatueDis, "Welcome: anonymous   State: disConnect");
        }

        //
        //Show About form tool
        //
        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.Show();
        }

        //
        //Connect tool
        //
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            toolStripButtonDisConnect.Enabled = true;
            toolStripButtonConnect.Enabled = false;
            connectToolStripMenuItem.Enabled = false;
            disConnnectToolStripMenuItem.Enabled = true;
            this.Cursor = Cursors.WaitCursor;
            SetSatusInfor(global::MyFtpSoft.Properties.Resources.link1,"[OPERATE]   ConnetingServer Plesse Wait...");
            Connect();
            
        }

        //
        //add new folder tool
        //
        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            string localPath = comboxLocal.Items[0].ToString();
            string fullPath = string.Empty;
            if (localPath != "Disk List")
            {
                AddFolderForm addFolder = new AddFolderForm(this,FolderSide.Local);
                addFolder.ShowDialog();
            }
            else 
            {
                MessageBox.Show("Path select error,Please choice an invadite one! ",
                    "Path error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //
        //Open file tool
        //
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (lvLocal.Focused)
            {
                string fullUrl = String.Empty;

                if (lvLocal.SelectedItems.Count > 0)
                {
                    if (!IsFolder(lvLocal.SelectedItems[0].Tag.ToString()))
                    {
                        Process.Start(lvLocal.SelectedItems[0].Tag.ToString());
                    }
                    else
                    {
                        MessageBox.Show("Selection is not a file!", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else 
                {
                    MessageBox.Show("You havenot choice a file to open", "No Choice",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (lvServer.Focused)
            {
              
            }
        }
        //
        //Excute Transfering
        //
        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            if (!isExcuteTransfering)
                ExcuteTranfer();
        }

        #endregion

        #region Menu Event

        #region ServerSite
        //
        //ShowMyServerControl Menu
        //
        private void menuItem11_Click(object sender, EventArgs e)
        {
            ServerControlForm serverControl = new ServerControlForm(this);
            serverControl.Show();
        }
        #endregion

        //
        //help Memu
        //
        private void menuItem64_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.Show();
        }

        #region Server
        //
        //Quit Menu
        //
        private void menuItem9_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region View
        //
        //Details Icon Menu
        //
        private void menuItem47_Click(object sender, EventArgs e)
        {
            lvLocal.View = View.Details;
            lvServer.View = View.Details;
            bigIconToolStripMenuItem.Checked = false;
            smallIconToolStripMenuItem1.Checked = false;
            listToolStripMenuItem1.Checked = false;
            detailsToolStripMenuItem1.Checked = true;
        }

        //
        //Larger Icon Menu
        //
        private void menuItem44_Click(object sender, EventArgs e)
        {
            lvLocal.View = View.LargeIcon;
            lvServer.View = View.LargeIcon;
            bigIconToolStripMenuItem.Checked = true;
            smallIconToolStripMenuItem1.Checked = false;
            listToolStripMenuItem1.Checked = false;
            detailsToolStripMenuItem1.Checked = false;
        }

        //
        //Small Icon Menu
        //
        private void menuItem45_Click(object sender, EventArgs e)
        {
            lvLocal.View = View.SmallIcon;
            lvServer.View = View.SmallIcon;
            bigIconToolStripMenuItem.Checked = false;
            smallIconToolStripMenuItem1.Checked = true;
            listToolStripMenuItem1.Checked = false;
            detailsToolStripMenuItem1.Checked = false;
        }

        //
        //List Icon Menu
        //
        private void menuItem46_Click(object sender, EventArgs e)
        {
            lvLocal.View = View.List;
            lvServer.View = View.List;
            bigIconToolStripMenuItem.Checked = false;
            smallIconToolStripMenuItem1.Checked = false;
            listToolStripMenuItem1.Checked = true;
            detailsToolStripMenuItem1.Checked = false;
        }
        //
        //Select All Menu
        //
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvLocal.Focused)
            {
                ListViewSelectAll(lvLocal);
            }
            else if (lvServer.Focused)
            {
                ListViewSelectAll(lvServer);
            }
        }

        //
        //Select Invert Menu
        //
        private void invertSelectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvLocal.Focused)
            {
                ListViewInvertSelect(lvLocal);
            }
            else if (lvServer.Focused)
            {
                ListViewInvertSelect(lvServer);
            }
        }

        #endregion

        #region Option
        //
        //Show Observe
        //
        private void shToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (shToolStripMenuItem.Checked)
                shToolStripMenuItem.Checked = false;
            else
                shToolStripMenuItem.Checked = true;
            if (shToolStripMenuItem.Checked)
            {
                rtxtLog.Visible = true;
                lvTransfer.Dock = DockStyle.Left;
            }
            else
            {
                rtxtLog.Visible = false;
                lvTransfer.Dock = DockStyle.Fill;
            }
        }
        //
        //Show Transfer
        //
        private void showTransferQueueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showTransferQueueToolStripMenuItem.Checked)
                showTransferQueueToolStripMenuItem.Checked = false;
            else
                showTransferQueueToolStripMenuItem.Checked = true;
            if (showTransferQueueToolStripMenuItem.Checked)
                lvTransfer.Visible = true;
            else
                lvTransfer.Visible = false;
        }
        //
        //Show Tool
        //
        private void showToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showToolsToolStripMenuItem.Checked)
                showToolsToolStripMenuItem.Checked = false;
            else
                showToolsToolStripMenuItem.Checked = true;
            if (showToolsToolStripMenuItem.Checked)
            {
                toolStrip1.Visible = true;
            }
            else
            {
                toolStrip1.Visible = false;
            }
        }
        #endregion

        #endregion

        #region Context Menu
        //
        //Locat Context Refresh
        //
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FillLocalView(ftpHelper1.Localfolder);
        }
        //
        //Server Context Refresh
        //
        private void refreshToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ftpHelper1.Dir();
        }
        //
        //Context local Large
        //
        private void largeIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvLocal.View = View.LargeIcon;
        }
        //
        //Context local Samll
        //
        private void smallIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvLocal.View = View.SmallIcon;
        }
        //
        //Context local List
        //
        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvLocal.View = View.List;
        }
        //
        //Context local Detail
        //
        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvLocal.View = View.Details;
        }
        #endregion

        private void Main_Load(object sender, EventArgs e)
        {
            InitilaProgressBar();
            
            GetControls();
            this.Enabled = false;
            InitializeLocalView();
            InitializeLoacalTree();
            InitialStatusBar();
            
            SwitchUpDownLoad(false);
            lsStatueServers = SerializeClass.DeSerializeServers();
            InitialMyServers();
            
         
            WelcomeForm wel = new WelcomeForm(this);
            wel.Show();
        }
       
       
        
        //
        //Remove file from the tansfer queue
        //
        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            if (lvTransfer.SelectedItems.Count > 0)
            {
                ListViewItem item = lvTransfer.SelectedItems[0];
                lvTransfer.Items.Remove(item);
            }
        }

        private void toolStripMenuItem27_Click(object sender, EventArgs e)
        {
            string deskTopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ftpHelper1.Localfolder = deskTopPath;
            ftpListViewLocal.SetUrl(deskTopPath);
            FillLocalView(deskTopPath);
        }

        private void toolStripMenuItem28_Click(object sender, EventArgs e)
        {

        }

       
        

        
    }
}
