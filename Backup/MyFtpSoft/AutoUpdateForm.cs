using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Common;
using System.Collections;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Net;

namespace MyFtpSoft
{
    public partial class AutoUpdateForm : Form
    {
        private static int availableUpdate = 0;
        private static XmlFiles updaterXmlFiles = null;
        private static string serverXmlFileUrl = String.Empty;
        private static string tempUpdatePath = string.Empty;
        private string mainAppExe = String.Empty;
        private bool isRun = false;
        private static ArrayList locateTempFileStringArray = new ArrayList();
        private static string locateTempNewConfigString = string.Empty;
        private static Hashtable htUpdateFile;
        public AutoUpdateForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            
        }

        public static int AutoCheckUpdate()
        {
            string localXmlFileUrl = "UpdateSoft.xml";
            string ServerUrl = String.Empty;

            try
            {
                updaterXmlFiles = new XmlFiles(localXmlFileUrl);
            }
            catch
            {
                MessageBox.Show("Update Config Error,please try setup!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            serverXmlFileUrl = updaterXmlFiles.GetNodeValue("//Url");
            AppUpdate appUpdate = new AppUpdate();
            appUpdate.UpdaterUrl = serverXmlFileUrl + "/Data/UpdateSoft.xml";

            try
            {
                tempUpdatePath = Environment.GetEnvironmentVariable("Temp") + "\\" + "_" + updaterXmlFiles.FindNode("//Application").Attributes["applicationId"].Value + "_" + "y" + "_" + "x" + "_" + "m" + "_" + "\\";
                appUpdate.DownAutoUpdateFile(tempUpdatePath);
                locateTempNewConfigString = tempUpdatePath + @"UpdateList.xml";
            }
            catch
            {
                MessageBox.Show("Connect to the server failure!", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);//5^1^a^s^p^x
                return -1;
            }

            htUpdateFile = new Hashtable();
            string path = tempUpdatePath + "UpdateList.xml";
            if (!File.Exists(path))
            {
                return -1;
            }
            string get = Application.StartupPath;
            string temp = get.Substring(0, get.LastIndexOf("\\"));
            string temp2 = temp.Substring(0, temp.LastIndexOf("\\"));
            get = temp2 + "\\Data\\UpdateSoft.xml";
            availableUpdate = appUpdate.CheckForUpdate(path, get, out htUpdateFile);

            return availableUpdate;
        }

        private void AutoUpdateForm_Load(object sender, EventArgs e)
        {
            panel2.Visible = false;
            btnFinish.Visible = false;
            Control.CheckForIllegalCrossThreadCalls = false;

            if (availableUpdate > 0)
            {
                for (int i = 0; i < htUpdateFile.Count; i++)
                {
                    string[] strArray=(string[])htUpdateFile[i];
                    ListViewItem item = new ListViewItem(strArray);
                    
                    lvUpdateList.Items.Add(item);
                }
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (availableUpdate > 0)
            {
                Thread thread = new Thread(new ThreadStart(DownUpdateFile));
                thread.IsBackground = true;
                thread.Start();
            }
            else 
            {
                MessageBox.Show("There are no useful updates!", "autoUpdate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }
        private void InvalidateControl()
        {
            panel2.Location = panel1.Location;
            panel2.Size = panel1.Size;
            panel1.Visible = false;
            panel2.Visible = true;

            btnNext.Visible = false;
            btnCancel.Visible = false;
            btnFinish.Location = btnCancel.Location;
            btnFinish.Visible = true;

        }

        private void DownUpdateFile()
        {
            mainAppExe = updaterXmlFiles.GetNodeValue("//EntryPoint");
            Process[] allProcess = Process.GetProcesses();
            foreach (Process p in allProcess)
            {

                if (p.ProcessName.ToLower() + ".exe" == mainAppExe.ToLower())
                {
                    for (int i = 0; i < p.Threads.Count; i++)
                        p.Threads[i].Dispose();
                    p.Kill();
                    isRun = true;
                    //break;
                }
            }
            for (int i = 0; i < this.lvUpdateList.Items.Count; i++)
            {
                string updateFile = lvUpdateList.Items[i].Text.Trim();
                
                string updateFileUrl = serverXmlFileUrl + lvUpdateList.Items[i].Text.Trim();
                long fileLength = 0;

                WebRequest webReq = WebRequest.Create(updateFileUrl);
                WebResponse webRes = webReq.GetResponse();
                fileLength = webRes.ContentLength;

                lbState.Text = "正在下载更新文件,请稍后...";
                pbDownFile.Value = 0;
                pbDownFile.Maximum = (int)fileLength;

                try
                {
                    Stream srm = webRes.GetResponseStream();
                    StreamReader srmReader = new StreamReader(srm);
                    byte[] bufferbyte = new byte[fileLength];
                    int allByte = (int)bufferbyte.Length;
                    int startByte = 0;
                    while (fileLength > 0)
                    {
                        Application.DoEvents();
                        int downByte = srm.Read(bufferbyte, startByte, allByte);
                        if (downByte == 0) { break; };
                        startByte += downByte;
                        allByte -= downByte;
                        pbDownFile.Value += downByte;

                        float part = (float)startByte / 1024;
                        float total = (float)bufferbyte.Length / 1024;
                        int percent = Convert.ToInt32((part / total) * 100);

                        this.lvUpdateList.Items[i].SubItems[2].Text = percent.ToString() + "%";
                    }

                    string tempPath = tempUpdatePath + updateFile;
                    
                    tempPath=tempPath.Replace("/", "\\");

                    CreateDirtory(tempPath);

                    locateTempFileStringArray.Add(tempPath);
                    FileStream fs = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
                    fs.Write(bufferbyte, 0, bufferbyte.Length);
                    srm.Close();
                    srmReader.Close();
                    fs.Close();
                }
                catch(Exception ex)
                {
                    MessageBox.Show("更新文件下载失败！" + ex.Message.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            InvalidateControl();
        }

        private void CreateDirtory(string path)
        {
            if (!File.Exists(path))
            {
                string[] dirArray = path.Split('\\');
                string temp = string.Empty;
                for (int i = 0; i < dirArray.Length - 1; i++)
                {
                    temp += dirArray[i].Trim() + "\\";
                    if (!Directory.Exists(temp))
                        Directory.CreateDirectory(temp);
                }
            }
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
            try
			{
				CopyFiles(Directory.GetCurrentDirectory());
				System.IO.Directory.Delete(tempUpdatePath,true);
                isRun = true;
			}
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            if (true == this.isRun) Process.Start(mainAppExe);
        }

        public void CopyFiles(string targetPath)
        {
            try
            {
                for (int i = 0; i < locateTempFileStringArray.Count; i++)
                {
                    string locateTempFileString = locateTempFileStringArray[i].ToString();
                    locateTempFileString = locateTempFileString.Replace("/", "\\");
                    if (locateTempFileString != string.Empty)
                    {
                        string leavePart = locateTempFileString.Substring(locateTempFileString.LastIndexOf("\\"));
                        string newtargetPath = targetPath.Substring(0, targetPath.LastIndexOf("\\")) + "\\Release" + leavePart;
                        File.Copy(locateTempFileString, newtargetPath, true);
                    }
                }
                if (locateTempNewConfigString != string.Empty)
                {
                    string leaveFtpBinPart = targetPath.Substring(0, targetPath.LastIndexOf("\\"));
                    string leaveFtpRootPart = leaveFtpBinPart.Substring(0, leaveFtpBinPart.LastIndexOf("\\"));
                    File.Copy(locateTempNewConfigString, leaveFtpRootPart + "\\Data\\UpdateSoft.xml", true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Main m = new Main();
            m.Show();
        }
    }
}
