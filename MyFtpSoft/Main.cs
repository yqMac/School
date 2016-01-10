﻿using System;
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
using System.Net;
using System.Text.RegularExpressions;

namespace MyFtpSoft
{
    public delegate void DelFtpEvent(MyFTPHelper.FtpEventArgs e);
    public partial class Main : Form
    {
        #region 变量
        public bool isExcuteTransfering   = false; //wether the system aready transfering

        private static string currentPath = "disk";
        private string ServerRoot         = string.Empty;
        private bool ServerRootFlag       = false;
        private ProgressBar barFileProg   = null;
        public List<RemoteServers> lsStatueServers;

        private Icon iconFolder           = null;
        private Label labelPrecent        = new Label();
        private string unExceptedMessage  = string.Empty;
        private bool isUnexcept         = false;
       
        

        ListView localLv;
        Label localLable;
        ComboBox localCombox;
        TreeView loacalTreeView;

        ListView serverLv;
        Label serverLable;
        ComboBox serverCombox;
        TreeView serverTreeView;

        RichTextBox rtxtLog;
        ListView transferLv;

        private bool signFlag = false;


        #endregion 变量

        public Main()
        {
            InitializeComponent();

            ftpHelper1.DelEvent += new MyFTPHelper.FtpEventHandler(ftp_DelEvent );
            iconFolder = global::MyFtpSoft.Properties.Resources.Folder;
        }

        #region 初始化方法

        /// <summary>
        /// 初始化进度条
        /// </summary>
        private void InitilaProgressBar()
        {
            barFileProg = new ProgressBar();
                 
            barFileProg.Style = ProgressBarStyle.Continuous;
            barFileProg.Width = 100;
          
            barFileProg.ForeColor = Color.Yellow;
            barFileProg.Minimum = 0;
            barFileProg.Maximum = 100;
            barFileProg.Value = 0;
        }

        /// <summary>
        /// 控件内控件外部化
        /// </summary>
        private void GetControls()
        {
            //get Control
            localLv = ftpListViewLocal.GetListView();
            localLable = ftpListViewLocal.GetLable();
            localCombox = ftpListViewLocal.GetComboBox();
            loacalTreeView = ftpListViewLocal.GetTreeView();

            serverLv = ftpListViewServer.GetListView();
            serverLable = ftpListViewServer.GetLable();
            serverCombox = ftpListViewServer.GetComboBox();
            serverTreeView = ftpListViewServer.GetTreeView();

            rtxtLog = ftpObserver1.GetRitchTextBox();
            transferLv = ftpObserver1.GetListView();

            //Set ImageList for listView
            localLv.SmallImageList = imageListLocal;
            localLv.LargeImageList = imageListLocal;
            serverLv.SmallImageList = imageListServer;
            serverLv.LargeImageList = imageListServer;
            loacalTreeView.ImageList = imageListLocalTree;
            serverTreeView.ImageList = imageListServerTree;
            //set ContextMenuStrip
            localLv.ContextMenuStrip = contextMenuStripLocal;
            serverLv.ContextMenuStrip = contextMenuStripServer;
           

            ftpListViewLocal.SetMain(this);
            ftpListViewServer.SetMain(this);
        }

        /// <summary>
        /// 状态栏初始化
        /// </summary>
        private void InitialStatusBar()
        {
            statusBar1.ShowPanels = true;
            statusBarPanelWelcome.Width = 350;

            SetConnetInfor( "欢迎: 未知   状态: 未连接");
            //SetConnetInfor(global::MyFtpSoft.Properties.Resources.StatueDis, 
            //  "Welcome: anonymous   State: disConnect");
            statusBarPanelStatus.Width =600;
        }

        /// <summary>
        /// 保存的服务器读取
        /// </summary>
        public void InitialMyServers()
        {
            HostList.DropDownItems.Clear();
            if (lsStatueServers != null && lsStatueServers.Count > 0)
            {
                ToolStripMenuItem toolStripItemNew;
                foreach (RemoteServers remoServer in lsStatueServers)
                { 
                    toolStripItemNew=new ToolStripMenuItem(remoServer.ServerName);
                    this.HostList.DropDownItems.AddRange(new ToolStripItem[] { toolStripItemNew });
                    toolStripItemNew.Click+=new EventHandler(RemoteServerConnet_Click);
                }
            }
        }


        /// <summary>
        /// 自定义添加菜单的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoteServerConnet_Click(object sender, EventArgs e)
        {
            ConnectByServerName(sender.ToString());
        }

        #endregion


        #region SamllMethod

       /// <summary>
       /// 根据连接状态控制菜单中上传、下载的可用性
       /// </summary>
       /// <param name="bSwitch"></param>
        private void SwitchUpDownLoad(bool bSwitch)
        {
            if (!bSwitch)
            {
                downLoadToolStripMenuItem.Enabled = false;
                uploadToolStripMenuItem.Enabled = false;
            }
            else
            {
                downLoadToolStripMenuItem.Enabled = true;
                uploadToolStripMenuItem.Enabled = true;
            }
        }

        /// <summary>
        /// 全选
        /// </summary>
        /// <param name="lv"></param>
        private void ListViewSelectAll(ListView lv)
        {
            for (int i = 0; i < lv.Items.Count; i++)
            {
                lv.Items[i].Selected = true;
            }
        }
        /// <summary>
        /// 反选
        /// </summary>
        /// <param name="lv"></param>
        private void ListViewInvertSelect(ListView lv)
        {
            for (int i = 0; i < lv.Items.Count; i++)
            {
                lv.Items[i].Selected = !lv.Items[i].Selected;
            }
        }

        /// <summary>
        /// 显示指定的连接信息
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="message"></param>
        private void SetConnetInfor( string message)
        {
            try
            {
                statusBarPanelWelcome.Text = message;
                //statusBarPanelWelcome.Icon = icon;
            }
            catch (Exception)
            { 
            //Ingor
            }
        }

        /// <summary>
        /// 判断文件是否在当前服务器工作目录中
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private ListViewItem JudgeServerExist(string fileName)
        {
            ListViewItem resultItem = null;
            foreach (ListViewItem item in serverLv.Items)
            {
                if (item.Text == fileName)
                {
                    resultItem = item;
                    break;
                }
            }
            return resultItem;
        }

       /// <summary>
       /// 显示指定的状态信息
       /// </summary>
       /// <param name="icon"></param>
       /// <param name="message"></param>
        private void SetSatusInfor(string message)
        {
            try
            {
                //statusBarPanelStatus.Icon = icon;
                statusBarPanelStatus.Text = message;
            }
            catch(Exception)
            {
                //Ignor
            }
        }

        /// <summary>
        /// 上传存在同名是否覆盖
        /// </summary>
        /// <param name="itemServer"></param>
        /// <param name="itemLocal"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 下载存在同名是否覆盖
        /// </summary>
        /// <param name="itemServer"></param>
        /// <returns></returns>
        private bool ExistDownLoadAsk(ListViewItem itemServer)
        {
            //get server same file Infor
            string serverFileSize = itemServer.SubItems[1].Text.ToString();
            string serverFileDate = itemServer.SubItems[3].Text.ToString();

            //get locat same fileInfo
            string localFileSize = "0";
            string locatFileDate = String.Empty;
            foreach (ListViewItem item in localLv.Items)
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

        /// <summary>
        /// 选中已保存服务器信息
        /// </summary>
        /// <param name="serverName"></param>
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
                    break;
                }
            }
        }

        /// <summary>
        /// listview分行样式显示
        /// </summary>
        /// <param name="lv"></param>
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

        /// <summary>
        /// 设置匿名登陆
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_anonymous_CheckedChanged(object sender, EventArgs e)
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


       /// <summary>
       /// 判断本地文件是否是文件夹
       /// </summary>
       /// <param name="dirName"></param>
       /// <returns></returns>
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

        /// <summary>
        /// 状态栏添加 文件传输进度条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="sbdevent"></param>
        private void statusBar1_DrawItem(object sender, StatusBarDrawItemEventArgs sbdevent)
        {
            barFileProg.Parent = this.statusBar1;
            labelPrecent.Parent = this.statusBar1;
            labelPrecent.BringToFront();
            labelPrecent.AutoSize = true;
            labelPrecent.BackColor = Color.Yellow;

            //barFileProg.SetBounds(statusBarPanelWelcome.Width, 2, statusBarPanelFileProgress.Width, this.statusBar1.ClientRectangle.Height - 2);
            //labelPrecent.SetBounds(statusBarPanelWelcome.Width + statusBarPanelFileProgress.Width/3, 5, 40, this.statusBar1.ClientRectangle.Height - 3);
        }

        /// <summary>
        /// 显示log信息
        /// </summary>
        /// <param name="showColor"></param>
        /// <param name="message"></param>
        private void AddMessageToObserve(Color showColor, string message)
        {
            rtxtLog.SelectionColor = showColor;
            if (message != String.Empty)
            {
                rtxtLog.AppendText(DateTime .Now.ToString ("hh:mm:ss:ffff ")+message);
            }
            rtxtLog.AppendText(Environment.NewLine);
            rtxtLog.SelectionStart = rtxtLog.TextLength;
            rtxtLog.ScrollToCaret();
        }

        /// <summary>
        /// 右下角信号标志变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [DebuggerStepThroughAttribute()]
        private void timer_sign_Tick(object sender, EventArgs e)
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


        #region toolbar的操作
        private void toolStripButton_Click(object sender, EventArgs e)
        {
            string tagstr = "";
            if(typeof(Button )==sender .GetType())
            {
                Button tmpbtn = (Button)sender;
                tagstr = tmpbtn.Tag.ToString();
            }else if (typeof (ToolStripButton )==sender.GetType())
            {
                ToolStripButton tmptsb = (ToolStripButton)sender;
                tagstr = tmptsb.Tag.ToString();
            }
            else if (typeof(ToolStripMenuItem ) == sender.GetType())
            {
                ToolStripMenuItem tmptsb = (ToolStripMenuItem)sender;
                tagstr = tmptsb.Tag.ToString();
            }

            //Button tsb = (Button)sender;
            if(tagstr!="")
            {
                switch (tagstr)
                {
                    case "refresh":
                        if (localLv.Focused)
                        {
                            FillLocalView(ftpHelper1.Localfolder);
                        }
                        else if (serverLv.Focused)
                        {
                            ftpHelper1.Dir();
                        }
                        break;
                    case "execute":
                        if (!isExcuteTransfering)
                            ExcuteTranfer();
                        break;
                    case "abor":
                        ftpHelper1.Abor();
                        break;
                    case "Connect":
                        timer1.Enabled = true;
                        btn_tool_disconnect.Enabled = true;
                        btn_tool_Connect.Enabled = false;
                        connectToolStripMenuItem.Enabled = false;
                        disConnnectToolStripMenuItem.Enabled = true;
                        this.Cursor = Cursors.WaitCursor;
                        SetSatusInfor("[操作]   链接服务器中 请稍候...");
                        Connect();
                        break;
                    case "disConnect":
                        SwitchUpDownLoad(false);
                        timer1.Enabled = false;
                        ServerRootFlag = false;
                        btn_tool_disconnect.Enabled = false;
                        btn_tool_Connect.Enabled = true;
                        connectToolStripMenuItem.Enabled = true;
                        disConnnectToolStripMenuItem.Enabled = false;
                        serverLv.Items.Clear();
                        ftpHelper1.DisConnect();
                        ftpHelper1.ClearCommandList();
                        SetConnetInfor("欢迎: 未知   状态: 未连接");
                        break;
                    case "manage":
                        ServerControlForm serverControl = new ServerControlForm(this);
                        serverControl.Show();
                        break;
                    case "quit":
                        this.Close();
                        break;
                }
            }
        }

        #endregion


        #region SelfOperate


        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    
 
        /// <summary>
        /// 删除本地选中项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteLocalItems_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < localLv.SelectedItems.Count; i++)
                {
                    if (localLv.SelectedItems[i].Text == "...")
                    {
                        MessageBox.Show("Invalid operate!", "invalid",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        ListViewItem item = localLv.SelectedItems[i];
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

        /// <summary>
        /// 添加文件到传输队列
        /// </summary>
        /// <param name="lvItem"></param>
        /// <param name="flag">0下载、1上传</param>
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

            string[] items = new string[6];
            items[0] = name;
            items[1] = fileSize;
            items[2] = "0";
            items[3] = target;
            items[4] = Guid.NewGuid().ToString();
            items[5] = "0";
            ListViewItem item = new ListViewItem(items, flag);
            transferLv.Items.Add(item);
        }
       
        /// <summary>
        /// 下载选中文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downLoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if (ftpHelper1.Localfolder == "Disk List")
            {
                MessageBox.Show("Url set error! Set a invadite url to save files ", "URL ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; i < serverLv.SelectedItems.Count; i++)
            {
                ListViewItem lvItemSelect = serverLv.SelectedItems[i];
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
            ExcuteTranfer();
            //if (!isExcuteTransfering)// not transfering the do it
            //{
            //    isExcuteTransfering = true;
               
            //}

        }

        /// <summary>
        /// 上传选中文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < localLv.SelectedItems.Count; i++)
            {

                ListViewItem selectItem = localLv.SelectedItems[i];
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
            ExcuteTranfer();
            //if (!isExcuteTransfering)// not transfering the do it
            //{
            //    isExcuteTransfering = true;
               
            //}
        }

        /// <summary>
        /// 执行传输队列
        /// </summary>
        private void ExcuteTranfer()
        {
            int worklength = transferLv.Items.Count >=3 ? 3 : transferLv.Items.Count;
            int workindex = 0;
            for (int i = 0; i < transferLv.Items.Count && workindex <worklength ; i++)
            {

                ListViewItem item = transferLv.Items[i];
                string id = item.SubItems[4].Text.ToString();
                string work = item.SubItems[5].Text.ToString();
                if (work == "1")
                {
                    workindex++;
                    continue;
                }
                else if (work == "0")
                {
                    string fileName = item.Text.ToString();
                    string sFileSize = item.SubItems[1].Text.ToString();
                    int ifileSize = Convert.ToInt32(sFileSize.Substring(0, sFileSize.Length - 2));

                    this.Cursor = Cursors.WaitCursor;
                    if (item.ImageIndex == 0) //begin downLoad
                    {
                        SetSatusInfor("[操作]   开始下载!  ...");
    
                        ftpHelper1.DowLoadFile(fileName, ifileSize, id);
                    }
                    else if (item.ImageIndex == 1)//begion upLoad
                    {
                        SetSatusInfor( "[操作]   开始上传!  ...");
                        ftpHelper1.UpLoadFile(fileName, ifileSize, id);
                    }
                    item.SubItems[5].Text = "1";
                    workindex++;
                    continue;
                }
                workindex = 3;
               
            }
            return;
        }

        /// <summary>
        /// 保存服务器信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            SerializeClass.SerializeObject(lsStatueServers);
        }
        

        /// <summary>
        /// 清空传输队列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearQueue(object sender, EventArgs e)
        {
            transferLv.Items.Clear();
        }

       /// <summary>
       /// 统计队列信息
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void queueInformation(object sender, EventArgs e)
        {
            int countUp = 0;
            int countDown = 0;
            for (int i = 0; i < transferLv.Items.Count; i++)
            {
                if (transferLv.Items[i].ImageIndex == 1)
                {
                    countUp++;
                }
                else if (transferLv.Items[i].ImageIndex == 0)
                {
                    countDown++;
                }
            }
            //QueueInformationForm qf = new QueueInformationForm(lvTransfer.Items.Count, countUp, countDown);
           // qf.Show();
        }

        /// <summary>
        /// 显示本地文件属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripShowProperties_Click(object sender, EventArgs e)
        {
            if (localLv.Focused)
            {
                string filePath = localLv.SelectedItems[0].Tag.ToString();
                Comm.ShowFileProperties(filePath);
            }
            else if (serverLv.Focused)
            {
                //not finish
            }
        }

        /// <summary>
        /// 添加选中项目到传输队列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripAddToQueue_Click(object sender, EventArgs e)
        {
            if (localLv.Focused)
            {
                for (int i = 0; i < localLv.SelectedItems.Count; i++)
                {

                    ListViewItem selectItem = localLv.SelectedItems[i];
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
            else if (serverLv.Focused)
            {
                if (ftpHelper1.Localfolder == "Disk List")
                {
                    MessageBox.Show("Url set error! Set a invadite url to save files ", "URL ERROR",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                for (int i = 0; i < serverLv.SelectedItems.Count; i++)
                {
                    ListViewItem lvItemSelect = serverLv.SelectedItems[i];
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
       
        /// <summary>
        /// 连接服务器
        /// </summary>
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

                    //ftpHelper1.Hostname = cbxServer.Text.Trim();
                    //ftpHelper1.Hostname = Dns.GetHostByName(cbxServer.Text.Trim()).AddressList[0].ToString();
                    if(!(new Regex(@"^(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[0-9]{1,2})(\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[0-9]{1,2})){3}$").IsMatch(cbxServer.Text .Trim ())))
                    ftpHelper1.Hostname = Dns.GetHostEntry(cbxServer.Text.Trim()).AddressList[0].ToString();
                    else
                    {
                        ftpHelper1.Hostname = cbxServer.Text.Trim();

                    }
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

        /// <summary>
        /// 本地双击_默认打开文件夹
        /// </summary>
        public void LocalDoubleClick()
        {
            if (localLv.SelectedItems[0].Tag.ToString() == "...")
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
                currentPath = localLv.SelectedItems[0].Tag.ToString();
                if (IsFolder(currentPath))
                {
                    FillLocalView(currentPath);
                    ftpListViewLocal.SetUrl(currentPath);
                    ftpHelper1.Localfolder = currentPath;
                }
            }

        }
        
        /// <summary>
        /// 服务器端双击_默认打开文件夹
        /// </summary>
        public void ServerDoublieClick()
        {
            //if (transferLv.Items.Count > 0)
            //{

            //}
            //else 
            {
                SetSatusInfor("[操作]   获取文件列表! ...");
                timer1.Enabled = true;
                if (serverLv.SelectedItems[0].Text == "...")
                {
                    this.Cursor = Cursors.WaitCursor;
                    ftpHelper1.DirUp();
                    ftpHelper1.Dir();
                }
                else
                {
                    string dirName = serverLv.SelectedItems[0].Text;
                    string isFolder = serverLv.SelectedItems[0].Tag.ToString();
                    if (isFolder == "1")
                    {
                        this.Cursor = Cursors.WaitCursor;
                        ftpHelper1.DirChange(dirName);
                        ftpHelper1.Dir();
                    }
                }
            }
        }

        /// <summary>
        /// 服务端删除文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < serverLv.SelectedItems.Count; i++)
            {
                if (serverLv.SelectedItems[0].Text == "...")
                {
                    MessageBox.Show("Invalid operate!", "invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (MessageBox.Show("Delete file or folder\"" + serverLv.SelectedItems[i].Text.ToString() + "\"?",
                        "Delete Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        //is file
                        if (serverLv.SelectedItems[i].Tag.ToString() == "0")
                        {
                            string fileName = serverLv.SelectedItems[i].Text;
                            ftpHelper1.DeleteFile(fileName);
                        }
                        //is folder
                        else
                        {
                            string folderName = serverLv.SelectedItems[0].Text;
                            ftpHelper1.DeleteFolder(ftpHelper1.Remotefolder + folderName);
                        }
                    }
                }
            }
            ftpHelper1.Dir();
        }
        
        /// <summary>
        /// 下载到桌面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deskTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string deskTopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ftpHelper1.Localfolder = deskTopPath;
            ftpListViewLocal.SetUrl(deskTopPath);
            FillLocalView(deskTopPath);

            for (int i = 0; i < serverLv.SelectedItems.Count; i++)
            {
                ListViewItem lvItemSelect = serverLv.SelectedItems[i];
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

        /// <summary>
        /// 下载到。。。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                for (int i = 0; i < serverLv.SelectedItems.Count; i++)
                {
                    ListViewItem lvItemSelect = serverLv.SelectedItems[i];
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
        /// <summary>
        /// 服务器端 创建文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddFolderForm addFolderForm = new AddFolderForm(this, FolderSide.Server);
            addFolderForm.Show();
        }
         
        /// <summary>
        /// 重命名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rnameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = serverLv.SelectedItems[0].Text;
            string oldeFileUrl = ftpHelper1.Remotefolder + fileName;
            RnameForm rnameForm = new RnameForm(oldeFileUrl, this);
            rnameForm.StartPosition = FormStartPosition.CenterScreen;
            rnameForm.Show();

        }

        #endregion

        #region load local File System

        /// <summary>
        /// 展开树形
        /// </summary>
        /// <param name="e"></param>
        public void ExpandTreeMethod(TreeViewEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            loacalTreeView.BeginUpdate();

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
            loacalTreeView.EndUpdate();
            this.Cursor = Cursors.Default;
        }

      /// <summary>
      /// 初始化本地文件树
      /// </summary>
        private void InitializeLoacalTree()
        {
            int idImage = 1;
            //string[] driverList = Directory.GetLogicalDrives();
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            for (int i = 0; i < allDrives.Length; i++)
            {
                DriveInfo drinfo = allDrives[i];
                if (!drinfo.IsReady)
                {
                    continue;
                }
                string diskName = string.Empty;
                diskName = drinfo.VolumeLabel + " (" + drinfo.Name + ")";
                Icon ico = ExtractIcon.GetIcon(drinfo.Name, true);
                imageListLocalTree.Images.Add(ico);
                int index = idImage++;
                TreeNode node = new TreeNode(diskName, index, index);
                node.Tag = drinfo.Name;
                loacalTreeView.Nodes.Add(node);

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
        
        /// <summary>
        /// 初始化本地文件listview
        /// </summary>
        private void InitializeLocalView()
        {
            this.Cursor = Cursors.WaitCursor;
            localLv.BeginUpdate();
            localLv.Items.Clear();
            imageListLocal.Images.Clear();

            ListViewItem item;
            int idImage = 0;
            System.IO.DriveInfo[] allDrives = System.IO.DriveInfo.GetDrives();
            string[] diskInfor;
            for (int i = 0; i < allDrives.Length; i++)
            {        
                DriveInfo drinfo = allDrives[i];
                if(!drinfo.IsReady)
                {
                    continue;
                }
                diskInfor = new string[4];
                diskInfor[0] = drinfo.VolumeLabel + " (" + allDrives[i].Name + ")";
                DirectoryInfo dr = new DirectoryInfo(drinfo.Name );
                string modiflyDate = dr.LastWriteTime.ToString();

                diskInfor[1] = String.Empty;
                diskInfor[2] = String.Empty;
                diskInfor[3] = modiflyDate;

                if (drinfo.IsReady)
                {
                    diskInfor[1] = (((drinfo.TotalSize / 1024) / 1024) / 1024).ToString() + "GB";
                    diskInfor[2] = drinfo.DriveFormat.ToString();
                }

                Icon ico = ExtractIcon.GetIcon(drinfo.Name , true);
                imageListLocal.Images.Add(ico);

                item = new ListViewItem(diskInfor, idImage++);
                item.Tag = drinfo.Name ;
                localLv.Items.Add(item);
            }
            localLv.EndUpdate();
            this.Refresh();
            this.Cursor = Cursors.Default;
            ftpListViewLocal.SetUrl("Disk List");
            ftpHelper1.Localfolder = "Disk List";
            localLable.Text = allDrives .Length + " Disk(s) , 0 File(s) , 0 Directories  [*.*]";

            ListViewStye(localLv);
        }

        /// <summary>
        /// 用path填充本地listview
        /// </summary>
        /// <param name="path"></param>
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
                    localLv.BeginUpdate();
                    localLv.Items.Clear();
                    imageListLocal.Images.Clear();
                    int idImage = 0;
                    Icon icoUp = global::MyFtpSoft.Properties.Resources.up2;
                    imageListLocal.Images.Add(icoUp);
                 
                    ListViewItem lvItemUp = new ListViewItem("...", idImage++);
                    lvItemUp.Tag = "...";
                    localLv.Items.Add(lvItemUp);

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
                        direArry[2] = "文件夹";
                        direArry[3] = direInfo.LastWriteTime.ToString();

                        Icon ico = ExtractIcon.GetIcon(direInfo.FullName, true);
                        imageListLocal.Images.Add(ico);
                       
                        item = new ListViewItem(direArry, idImage++);
                        item.Tag = direInfo.FullName;
                        localLv.Items.Add(item);
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
                        localLv.Items.Add(item);
                    }
                    #endregion

                    
                    ListViewStye(localLv);
                    localLv.EndUpdate();
                    this.Refresh();
                    this.Cursor = Cursors.Default;

                    localLable.Text = "0 Disk(s) , " + fileList.Length + " File(s) , " + directoryList.Length + " Directories  [*.*]";
                }
            }
            catch (Exception)
            {
                localLv.EndUpdate();
                this.Refresh();
                this.Cursor = Cursors.Default;
            }

        }

     

        #endregion
      
        #region FtpEvent

        /// <summary>
        /// 方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ftp_DelEvent(object sender,FtpEventArgs e)
        {
            DelFtpEvent delFtpEvent = new DelFtpEvent(ftpDelEvent);
            this.Invoke(delFtpEvent,e);
        }

        /// <summary>
        /// ftp事件处理
        /// </summary>
        /// <param name="e"></param>
        private void ftpDelEvent(FtpEventArgs e)
        {

            switch (e.Type)
            {
                case enumFtpEventArgsTypes.Error:
                    AddMessageToObserve(Color.Red, e.Function + "-" + e.SMessage);
                    break;
                case enumFtpEventArgsTypes.Message:
                    AddMessageToObserve(Color.Green, e.SMessage);
                    break;
                case enumFtpEventArgsTypes.ConnectCommpleted:
                    #region Set StateBar
                    SetConnetInfor("欢迎: 未知   状态: 已连接");
                    SetSatusInfor("登陆中! ...");
                    #endregion
                    SwitchUpDownLoad(true);
                    ftpHelper1.Login();
                    break;
                case enumFtpEventArgsTypes.LoginCommpleted:
                    SetConnetInfor(string.Format("欢迎: {0}   状态: 已连接", ftpHelper1.Username));
                    SetSatusInfor( "[操作]   获取文件列表 ...");
                    ftpHelper1.Dir();
                    break;
                case enumFtpEventArgsTypes.CommandCommpleted:
                    AddMessageToObserve(Color.Blue, e.SMessage);
                    break;
                case enumFtpEventArgsTypes.BaseInfor:
                    AddMessageToObserve(Color.Green, e.SMessage);
                    break;
                case enumFtpEventArgsTypes.DirCommpleted:
                    try
                    {
                        int fileCount = 0;
                        int directoryCount = 0;
                        int i = 0;
                        int imageIndex = 0;
                        string isFolder = "0";

                        #region Add TransferMsg
                        string msgDir = "\r\nTransfered " + e.Totalbytes.ToString() + " bytes in " +
                            ((float)e.Timeelapsed / 1000).ToString() + " seconds" + "\r\n";
                        AddMessageToObserve(Color.Yellow, msgDir);
                        #endregion

                        if (!ServerRootFlag)
                        {
                            ServerRoot = ftpHelper1.Remotefolder;
                            ServerRootFlag = true;
                            InitializeServerTree(ServerRoot);
                        }


                        serverLv.BeginUpdate();
                        serverLv.Items.Clear();
                        imageListServer.Images.Clear();

                        #region Add Back
                        if (ftpHelper1.Remotefolder != ServerRoot)
                        {
                            Icon icoUp = global::MyFtpSoft.Properties.Resources.up2;
                            imageListServer.Images.Add(icoUp);
                            ListViewItem lvItemUp = new ListViewItem("...", imageIndex++);
                            lvItemUp.Tag = "...";
                            serverLv.Items.Add(lvItemUp);

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
                            serverLv.Items.Add(lvItem);
                        }
                        #endregion

                        #region add others
                        ftpListViewServer.SetUrl(ftpHelper1.Remotefolder);
                        serverLable.Text = "0 Disk(s) , " + fileCount + " File(s) , " + directoryCount +
                                         " Directories  " + e.Totalbytes.ToString() + " bytes offer [*.*]";
                        ListViewStye(serverLv);
                        timer1.Enabled = false;
                        statusBarPanelSign.Icon = global::MyFtpSoft.Properties.Resources.Gray;
                        #endregion

                        serverLv.EndUpdate();
                        if (!isUnexcept)//Sucessfully
                        {
                            SetSatusInfor( "[操作]   文件列表获取完毕! ...");
                        }
                        else //Failure  unexcepted
                        {
                            SetSatusInfor( unExceptedMessage);
                            isUnexcept = false;
                        }
                        this.Cursor = Cursors.Default;
                        ftpHelper1.listServerData.Clear();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    break;
                case enumFtpEventArgsTypes.FileDownLoadCommpleted:
                    string msgFDC = "FileDownload Transfered " + e.Totalbytes.ToString() + " bytes in " +
        ((float)e.Timeelapsed / 1000).ToString() + " seconds" + "\r\n";
                    AddMessageToObserve(Color.Yellow, msgFDC);
                    FillLocalView(ftpHelper1.Localfolder);
                    this.Cursor = Cursors.Default;
                    timer1.Enabled = false;
                    //transferLv.Items.RemoveAt(0);
                    for (int i = 0; i < transferLv.Items.Count; i++)
                    {
                        if (e.Workid == transferLv.Items[i].SubItems[4].Text) { transferLv.Items.RemoveAt(i); break; }
                    }
                    if (transferLv.Items.Count > 0)
                    {
                        ExcuteTranfer();
                    }
                    else
                    {
                        isExcuteTransfering = false;
                        SetSatusInfor("[操作]   下载完毕! ");
                    }
                    break;
                case enumFtpEventArgsTypes.FileUpLoadCommpleted:
                    string msg = "FileUpload Transfered " + e.Totalbytes.ToString() + " bytes in " +
             ((float)e.Timeelapsed / 1000).ToString() + " seconds" + "\r\n";
                    AddMessageToObserve(Color.Yellow, msg);
                    ftpHelper1.Dir();
                    this.Cursor = Cursors.Default;
                    timer1.Enabled = false;
                    //transferLv.Items.RemoveAt(0);
                    for (int i = 0; i < transferLv.Items.Count; i++)
                    {
                        if (e.Workid == transferLv.Items[i].SubItems[4].Text) { transferLv.Items.RemoveAt(i); break; }
                    }
                    if (transferLv.Items.Count > 0)
                    {
                        ExcuteTranfer();
                    }
                    else
                    {
                        isExcuteTransfering = false;
                        SetSatusInfor( "[操作]   上传完毕! ");
                    }
                    break;
                case enumFtpEventArgsTypes.ResponseUnexpected:
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
                    break;
                case enumFtpEventArgsTypes.FilePassProgressCommpleted:
                    float  precent = (((float)e.AreadyBytes / (float)e.Totalbytes) * 100);
                    string pro = precent.ToString("F4");
                    //string workid = e.SMessage ;
                    for (int i = 0; i < transferLv.Items.Count; i++)
                    {
                        if (e.SMessage == transferLv.Items[i].SubItems[4].Text) { transferLv.Items[i].SubItems [2].Text =pro ; break; }
                    }
                    //barFileProg.Value = precent;
                    //labelPrecent.Text = precent.ToString() + "% Finished";
                    //statusBarPanelStatus.Text = "Transfer:" + e.SMessage + " Time: " + ((float)e.Timeelapsed / 1000).ToString() + " (s)  ";

                    break;
                default:
                    break;
            }
        }

       
        private void InitializeServerTree(string root)
        {
            //treeViewServer.Nodes.Clear();
            TreeNode nodeRoot = new TreeNode(root, 0, 0);
            nodeRoot.Tag = root;
            serverTreeView.Nodes.Add(nodeRoot);
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
            serverTreeView.SelectedNode.Nodes.Clear();
            serverTreeView.SelectedNode.ImageIndex = 0;
            serverTreeView.SelectedNode.SelectedImageIndex = 0;
            for (int i = 0; i < ftpHelper1.listServerData.Count; i++)
            {
                {
                    TreeNode newNode = null;
                    if (ftpHelper1.listServerData[i].isDirectory)
                    {
                        newNode = new TreeNode(ftpHelper1.listServerData[i].fileName, 1, 1);
                        if (serverCombox.SelectedItem.ToString().EndsWith("/"))
                        {
                            newNode.Tag = serverCombox.SelectedItem.ToString() + ftpHelper1.listServerData[i].fileName;
                        }
                        else
                        {
                            newNode.Tag = serverCombox.SelectedItem.ToString() + "/" + ftpHelper1.listServerData[i].fileName;
                        }
                        serverTreeView.SelectedNode.Nodes.Add(newNode);
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



       /// <summary>
       /// 本地创建文件夹
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void localMKDIR_Click(object sender, EventArgs e)
        {
            string localPath = localCombox.Items[0].ToString();
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

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFile_Click(object sender, EventArgs e)
        {
            if (localLv.Focused)
            {
                string fullUrl = String.Empty;

                if (localLv.SelectedItems.Count > 0)
                {
                    if (!IsFolder(localLv.SelectedItems[0].Tag.ToString()))
                    {
                        Process.Start(localLv.SelectedItems[0].Tag.ToString());
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
            else if (serverLv.Focused)
            {
              
            }
        }
        #endregion

        #region Menu Event


        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    

        #region View
       
        /// <summary>
        /// 详细信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void detail_Click(object sender, EventArgs e)
        {
            localLv.View = View.Details;
            serverLv.View = View.Details;
            bigIconToolStripMenuItem.Checked = false;
            smallIconToolStripMenuItem1.Checked = false;
            listToolStripMenuItem1.Checked = false;
            detailsToolStripMenuItem1.Checked = true;
        }

        /// <summary>
        /// 大图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void largeIcon_Click(object sender, EventArgs e)
        {
            localLv.View = View.LargeIcon;
            serverLv.View = View.LargeIcon;
            bigIconToolStripMenuItem.Checked = true;
            smallIconToolStripMenuItem1.Checked = false;
            listToolStripMenuItem1.Checked = false;
            detailsToolStripMenuItem1.Checked = false;
        }

        /// <summary>
        /// 小图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void smallIcon_Click(object sender, EventArgs e)
        {
            localLv.View = View.SmallIcon;
            serverLv.View = View.SmallIcon;
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
            localLv.View = View.List;
            serverLv.View = View.List;
            bigIconToolStripMenuItem.Checked = false;
            smallIconToolStripMenuItem1.Checked = false;
            listToolStripMenuItem1.Checked = true;
            detailsToolStripMenuItem1.Checked = false;
        }
        
        /// <summary>
        /// 全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (localLv.Focused)
            {
                ListViewSelectAll(localLv);
            }
            else if (serverLv.Focused)
            {
                ListViewSelectAll(serverLv);
            }
        }

        /// <summary>
        /// 反选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void invertSelectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (localLv.Focused)
            {
                ListViewInvertSelect(localLv);
            }
            else if (serverLv.Focused)
            {
                ListViewInvertSelect(serverLv);
            }
        }

        #endregion

        #region Option
        /// <summary>
        /// 显示日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void shToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (shToolStripMenuItem.Checked)
                shToolStripMenuItem.Checked = false;
            else
                shToolStripMenuItem.Checked = true;
            if (shToolStripMenuItem.Checked)
            {
                rtxtLog.Visible = true;
                transferLv.Dock = DockStyle.Left;
            }
            else
            {
                rtxtLog.Visible = false;
                transferLv.Dock = DockStyle.Fill;
            }
        }
        /// <summary>
        /// 显示队列信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showTransferQueueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showTransferQueueToolStripMenuItem.Checked)
                showTransferQueueToolStripMenuItem.Checked = false;
            else
                showTransferQueueToolStripMenuItem.Checked = true;
            if (showTransferQueueToolStripMenuItem.Checked)
                transferLv.Visible = true;
            else
                transferLv.Visible = false;
        }
        /// <summary>
        /// 显示工具栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        
        /// <summary>
        /// 刷新本地
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FillLocalView(ftpHelper1.Localfolder);
        }
        
        /// <summary>
        /// 服务器刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ftpHelper1.Dir();
        }
        
        /// <summary>
        /// 更改显示方式_toolStrip版
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void viewToolStripMenu_Click(object sender,EventArgs e)
        {
            ToolStripMenuItem tsmi = (ToolStripMenuItem)sender;
            if (tsmi == null || tsmi .Tag ==null ) return;
            View nV = View.Details;
            switch (tsmi.Tag.ToString()) {
                case "details":
                    nV = View.Details;
                    break;
                case "large":
                    nV = View.LargeIcon;
                    break;
                case "small":
                    nV = View.SmallIcon ;
                    break;
                case "list":
                    nV = View.List;
                    break;
                default:
                    nV = View.Details;
                    break;

            }
            if (localLv.Focused)
                localLv.View = nV ;
            else
                serverLv.View = nV ;

        }
       
        #endregion

        private void Main_Load(object sender, EventArgs e)
        {
            //InitilaProgressBar();
            
            GetControls();

            this.Enabled = false;

            InitializeLocalView();
            InitializeLoacalTree();
            InitialStatusBar();
            
            SwitchUpDownLoad(false);
            lsStatueServers = SerializeClass.DeSerializeServers();
            InitialMyServers();

            this.Enabled = true;
            //WelcomeForm wel = new WelcomeForm(this);
            //wel.Show();
        }
       
  
        /// <summary>
        /// 切换到桌面目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deskTopPath_Click(object sender, EventArgs e)
        {
            string deskTopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ftpHelper1.Localfolder = deskTopPath;
            ftpListViewLocal.SetUrl(deskTopPath);
            FillLocalView(deskTopPath);
        }

    }
}
