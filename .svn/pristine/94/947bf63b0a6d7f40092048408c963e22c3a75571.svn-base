using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Model;

namespace MyFtpSoft
{
    public partial class ServerControlForm : Form
    {
        private Main _main;
        private RemoteServers remoteServer;
        public ServerControlForm(Main main)
        {
            InitializeComponent();
            this._main = main;
            
            
        }
        private void InitialTreeView()
        {
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(new TreeNode("My Servers",0,0));
            
            TreeNode node;
            if (_main.lsStatueServers != null)
            {
                foreach (RemoteServers remoteServer in _main.lsStatueServers)
                {
                    node = new TreeNode(remoteServer.ServerName, 1, 1);
                    node.Tag = remoteServer.ServerName;
                    node.ToolTipText = remoteServer.Trag.ToString();
                    treeView1.Nodes[0].Nodes.Add(node);
                }

                treeView1.ExpandAll();
            }
            else 
            {
                MessageBox.Show("File not exist, System will create a new one!", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
       
        private void ServerControlForm_Load(object sender, EventArgs e)
        {
            InitialTreeView();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxAnonymous.Checked)
            {
                txtUserID.Text = "anonymous";
                txtUserPwd.Text = "anonymous";
                txtUserID.ReadOnly = true;
                txtUserPwd.ReadOnly = true;
            }
            else
            {
                txtUserID.Text = String.Empty;
                txtUserPwd.Text = String.Empty;
                txtUserID.ReadOnly = false;
                txtUserPwd.ReadOnly = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (_main.lsStatueServers == null)
                    _main.lsStatueServers = new List<RemoteServers>();
                if (txtUserID.Text.Trim() == String.Empty
                  || txtServerIp.Text.Trim() == String.Empty
                  || txtPort.Text.Trim() == String.Empty)
                {
                    MessageBox.Show("Information deformity!", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    if (!JudgeExistServerName(txtServerName.Text.Trim()))
                    {
                        
                        remoteServer = new RemoteServers();
                        remoteServer.ServerName = txtServerName.Text.Trim();
                        remoteServer.ServerIP = txtServerIp.Text.Trim();
                        remoteServer.UserID = txtUserID.Text.Trim();
                        if (cbxRemberPwd.Checked)
                        {
                            remoteServer.UserPwd = txtUserPwd.Text.Trim();
                        }
                        remoteServer.ServerPort = Convert.ToInt32(txtPort.Text.Trim());
                        remoteServer.LocalUrl = txtLocalUrl.Text.Trim();
                        remoteServer.ServerUrl = txtServerUrl.Text.Trim();
                        remoteServer.Trag = txtTrag.Text.Trim();
                        if (cbxAnonymous.Checked)
                        {
                            remoteServer.IsAnonymous = 1;
                        }
                        else 
                        {
                            remoteServer.IsAnonymous = 0;
                        }
                        _main.lsStatueServers.Add(remoteServer);
                        InitialTreeView();
                        _main.InitialMyServers();
                        ClearText();
                        MessageBox.Show("AddServer Sucessfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        remoteServer = GetServerByName(txtServerName.Text.Trim());
                        if (remoteServer != null)
                        {
                            remoteServer.ServerName = txtServerName.Text.Trim();
                            remoteServer.ServerIP = txtServerIp.Text.Trim();
                            remoteServer.UserID = txtUserID.Text.Trim();
                            if (cbxRemberPwd.Checked)
                            {
                                remoteServer.UserPwd = txtUserPwd.Text.Trim();
                            }
                            remoteServer.ServerPort = Convert.ToInt32(txtPort.Text.Trim());
                            remoteServer.LocalUrl = txtLocalUrl.Text.Trim();
                            remoteServer.ServerUrl = txtServerUrl.Text.Trim();
                            remoteServer.Trag = txtTrag.Text.Trim();
                            if (cbxAnonymous.Checked)
                            {
                                remoteServer.IsAnonymous = 1;
                            }
                            else
                            {
                                remoteServer.IsAnonymous = 0;
                            }
                            UpdateServer(remoteServer);
                            MessageBox.Show("Update Sucessfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("The IpAdress or the port is not the correct formate!",
                           "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "UnkonwError", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void ClearText()
        {
            txtServerName.Text =String.Empty;
            txtServerIp.Text = String.Empty;
            txtUserID.Text = String.Empty;
            txtUserPwd.Text = String.Empty;
            txtPort.Text=String.Empty;
            txtLocalUrl.Text=String.Empty;
            txtServerUrl.Text=String.Empty;
            txtTrag.Text=String.Empty;
        }

        private void UpdateServer(RemoteServers server)
        {
            try
            {
                for (int i = 0; i < _main.lsStatueServers.Count; i++)
                {
                    if (_main.lsStatueServers[i].ServerName == server.ServerName)
                    {
                        _main.lsStatueServers[i].ServerName = server.ServerName;
                        _main.lsStatueServers[i].ServerIP = server.ServerIP;
                        _main.lsStatueServers[i].UserID = server.UserID;
                        _main.lsStatueServers[i].UserPwd =server.UserPwd;
                        _main.lsStatueServers[i].ServerPort = Convert.ToInt32(server.ServerPort);
                        _main.lsStatueServers[i].LocalUrl = server.LocalUrl;
                        _main.lsStatueServers[i].ServerUrl = server.ServerUrl;
                        _main.lsStatueServers[i].Trag = server.Trag;
                        _main.lsStatueServers[i].IsAnonymous = server.IsAnonymous;
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("The IpAdress or the port is not the correct formate!",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "UnkonwError", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private bool JudgeExistServerName(string serverName)
        {
            if (_main.lsStatueServers == null)
                return false;
            bool result = false;
            
            foreach (RemoteServers remoteServers in _main.lsStatueServers)
            {
                if (remoteServers.ServerName == serverName)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private bool RemoveServerByName(string serverName)
        {
            bool result = false;
            foreach (RemoteServers remoteServer in _main.lsStatueServers)
            {
                if (remoteServer.ServerName == serverName)
                {
                    _main.lsStatueServers.Remove(remoteServer);
                    result = true;
                    break;
                }
            }
            return result;
        }

        private RemoteServers GetServerByName(string serverName)
        {
            RemoteServers server=null;
            foreach (RemoteServers getServer in _main.lsStatueServers)
            {
                if (getServer.ServerName== serverName)
                {
                    server = getServer;
                }
            }
            return server;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode != treeView1.Nodes[0]) 
                if (RemoveServerByName(treeView1.SelectedNode.Tag.ToString()))
                {
                    InitialTreeView();
                    _main.InitialMyServers();
                    MessageBox.Show("Remove Sucessfully!", "Sucess", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ClearText();
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeView1.SelectedNode != null && treeView1.SelectedNode != treeView1.Nodes[0])
            {
                string getServerName = treeView1.SelectedNode.Tag.ToString();
                RemoteServers server = GetServerByName(getServerName);
                if (server != null)
                {
                    txtServerName.Text = server.ServerName;
                    txtServerIp.Text = server.ServerIP;
                    txtUserID.Text = server.UserID;
                    txtUserPwd.Text = server.UserPwd;
                    txtPort.Text = server.ServerPort.ToString();
                    txtLocalUrl.Text = server.LocalUrl;
                    txtServerUrl.Text = server.ServerUrl;
                    txtTrag.Text = server.Trag;
                    if (server.IsAnonymous == 0)
                    {
                        cbxRemberPwd.Checked = false;
                    }
                    else if (server.IsAnonymous == 1)
                    {
                        cbxRemberPwd.Checked = true;
                    }
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.ShowNewFolderButton = false;
            fbd.Description = "Select a folder that will remind when connect";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                if (remoteServer != null)
                {
                    txtLocalUrl.Text = fbd.SelectedPath;
                    remoteServer.LocalUrl = fbd.SelectedPath;
                    remoteServer.ServerUrl = txtServerUrl.Text.Trim();
                }
            }
        }
    }
}
