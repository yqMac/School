using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MyFTPHelper
{
    public delegate void FtpEventHandler(object sender,FtpEventArgs e);

    public partial class FtpHelper : Component
    {
        /// <summary>
        /// 处理工具对象
        /// </summary>
        FtpHandler _ftpHandler;
        /// <summary>
        /// 用户名
        /// </summary>
        private string _username;
        /// <summary>
        /// 密码
        /// </summary>
        private string _password;
        /// <summary>
        /// 服务器地址
        /// </summary>
        private string _hostname;
        /// <summary>
        /// 端口
        /// </summary>
        private int _port;
        /// <summary>
        /// 本地目录
        /// </summary>
        private string _localfolder;
        /// <summary>
        /// 服务器工作目录
        /// </summary>
        private string _remotefolder;

        /// <summary>
        /// 传输类型
        /// </summary>
        private string _type;
        /// <summary>
        /// 标识当前是主控对象还是下载线程
        /// </summary>
        public bool isThreadForTransfer = false;

        /// <summary>
        /// 服务器文件数据
        /// </summary>
        public List<ServerFileData> listServerData = new List<ServerFileData>();

      

        public FtpHelper()
        {
            InitializeComponent();

            _username = string.Empty;
            _password = string.Empty;
            _hostname = string.Empty;
            _port = 21;								// Default port
            _localfolder = "c:\\temp";					// Local folder !! TO DO
            _remotefolder = string.Empty;
            _type = "A";
            _ftpHandler = new FtpHandler(this);
        }

        public FtpHelper(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
            _username = string.Empty;
            _password = string.Empty;
            _hostname = string.Empty;
            _port = 21;								// Default port
            _localfolder = "c:\\temp";					// Local folder !! TO DO
            _remotefolder = string.Empty;
            _type = "A";
            _ftpHandler = new FtpHandler(this);

        }

        public void Connect()
        {
            _ftpHandler.Connect(_hostname, _port);
        }

        #region Attrbute
        [
        Category("Account"),
        Description("FTP account username"),
        ]
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }
        [
        Category("Account"),
        Description("FTP account password"),
        ]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        [
        Category("FTP Server"),
        Description("FTP server hostname"),
        ]
        public string Hostname
        {
            get { return _hostname; }
            set { _hostname = value; }
        }
        [
        Category("FTP Server"),
        Description("FTP Server port"),
        DefaultValue(22)
        ]
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }
        [
        Category("Directory"),
        Description("Local directory"),
        ]
        public string Localfolder
        {
            get { return _localfolder; }
            set { _localfolder = value; }
        }
        [
        Category("Directory"),
        Description("Remote directory"),
        ]
        public string Remotefolder
        {
            get { return _remotefolder; }
            set { _remotefolder = value; }
        }

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
        #endregion

        #region EventHandler

        public event FtpEventHandler DelEvent;


        public void OnResponseUnexpected(FtpEventArgs e)
        {
            e.Type = enumFtpEventArgsTypes.ResponseUnexpected;
            if (DelEvent != null)
            {
                DelEvent(this, e);
            }
        }
        public void OnFilePassProgressCommpleted(FtpEventArgs e)
        {
            e.Type = enumFtpEventArgsTypes.FilePassProgressCommpleted;
            // if (FileDownLoadCommpleted != null)
            {
                DelEvent(this, e);
            }
        }

        /// <summary>
        /// 当前事件标志
        /// </summary>
        public EventWaitHandle _handler = null;

        public void OnFileDownLoadCommpleted(FtpEventArgs e)
        {
            e.Type = enumFtpEventArgsTypes.FileDownLoadCommpleted;
            if (!isThreadForTransfer)
            {
                DelEvent(this, e);
            }
            else
            {
                if (this._handler != null)
                {
                    this.DisConnect();

                    this._handler.Set();

                    this.Dispose();
                }
            }
        }
        public void OnFileUpLoadCommpleted(FtpEventArgs e)
        {
            e.Type = enumFtpEventArgsTypes.FileUpLoadCommpleted;
            if (!isThreadForTransfer )
            {
                DelEvent(this, e);
            }
            else
            {
                if (this._handler != null)
                {
                    this.DisConnect();

                    this._handler.Set();

                    this.Dispose();
                }
            }
        }
        public void OnMessage(FtpEventArgs e)
        {
            e.Type = enumFtpEventArgsTypes.Message;
            if (DelEvent != null)
            {
                DelEvent(this, e);
            }
        }
        public void OnConnectCommpleted(FtpEventArgs e)
        {
            e.Type = enumFtpEventArgsTypes.ConnectCommpleted;
            if (!isThreadForTransfer &&DelEvent != null)
            {
                DelEvent(this, e);
            }
        }
        public void OnError(FtpEventArgs e)
        {
            if (isThreadForTransfer)
            {
                this.DisConnect();
                this._handler.Set();
            }
            e.Type = enumFtpEventArgsTypes.Error;
            if (DelEvent != null)
            {
                DelEvent(this, e);
            }
        }
        public void OnLoginFailure(FtpEventArgs e)
        {
            e.Type = enumFtpEventArgsTypes.LoginFailure;
            if (DelEvent != null)
            {
                DelEvent(this, e);
            }
        }
        public void OnLoginCommpleted(FtpEventArgs e)
        {
            e.Type = enumFtpEventArgsTypes.LoginCommpleted;
            if (DelEvent != null)
            {
                DelEvent(this, e);
            }
        }
        public void OnCommandCommpleted(FtpEventArgs e)
        {
            e.Type = enumFtpEventArgsTypes.CommandCommpleted;
            if (DelEvent != null)
            {
                DelEvent(this, e);
            }
        }
        public void OnBaseInfor(FtpEventArgs e)
        {
            e.Type = enumFtpEventArgsTypes.BaseInfor;
            if (DelEvent != null)
            {
                DelEvent(this, e);
            }
        }
        public void OnDirCommpleted(FtpEventArgs e)
        {
            e.Type = enumFtpEventArgsTypes.DirCommpleted;
            if (DelEvent != null)
            {
                ParseDirList(e.SMessage);
                DelEvent(this, e);
            }
        }

        #endregion

        public void Login()
        {
            if (_username != string.Empty && _password != String.Empty)
            {
                _ftpHandler.PrepareCommand(new FtpCommand("USER", 331, _username));
                _ftpHandler.PrepareCommand(new FtpCommand("PASS", 230, _password));
                _ftpHandler.PrepareCommand(new FtpCommand("SYST", 0));
                _ftpHandler.PrepareCommand(new FtpCommand("PWD", 257));
                _ftpHandler.PrepareCommand(new FtpCommand("TYPE", 200, "I"));
                HeartBeat();
            }
        }

        public void Dir()
        {
            _ftpHandler.PrepareCommand(new FtpCommand("TYPE A", 200));
            _ftpHandler.PrepareCommand(new FtpCommand("LIST", 226));
        }

        private void ParseDirList(string sDir)
        {
            listServerData.Clear();
            if (sDir.Trim() == string.Empty)
            {
                return;
            }

            const string CRLF = "\r\n";
            sDir = sDir.Replace(CRLF, "\r");
            sDir = sDir.Trim();
            string[] StrFileArray = sDir.Split('\r');
            int autodetect = 0;
            ServerFileData sd = null;
            foreach (string fileLine in StrFileArray)
            {
                if (autodetect == 0)
                {
                    sd = PraseDosDirLine(fileLine);
                    if (sd == null)
                    {
                        sd = ParseUnixDirLine(fileLine);
                        autodetect = 2;
                    }
                    else
                    {
                        autodetect = 1;
                    }
                }
                else if (autodetect == 1)
                {
                    sd = PraseDosDirLine(fileLine);
                }
                else if (autodetect == 2)
                {
                    sd = ParseUnixDirLine(fileLine);
                }
                if (sd != null)
                {
                    listServerData.Add(sd);
                }
            }
        }
        //
        //Prase the Dos file Line infor
        //
        private ServerFileData PraseDosDirLine(string fileLine)
        {
            ServerFileData serverData = new ServerFileData();
            try
            {

                string[] fileInfo = new string[3];
                int index = 0;
                int pos = 0;
                pos = fileLine.IndexOf(' ');
                while (index < fileInfo.Length)
                {
                    fileInfo[index] = fileLine.Substring(0, pos);
                    fileLine = fileLine.Substring(pos);
                    fileLine = fileLine.Trim();
                    index++;
                    pos = fileLine.IndexOf(' ');
                }

                serverData.fileName = fileLine;
                if (fileInfo[2] != "<DIR>")
                {
                    serverData.size = Convert.ToInt32(fileInfo[2]);
                }
                serverData.date = fileInfo[0] + " " + fileInfo[1];
                serverData.isDirectory = fileInfo[2] == "<DIR>";

                return serverData;
            }
            catch
            {
                serverData = null;
                return serverData;
            }
        }
        private ServerFileData ParseUnixDirLine(string fileLine)
        {
            ServerFileData serverData = new ServerFileData();
            string[] fileInfo = new string[8];
            int index = 0;
            int position;

            position = fileLine.IndexOf(' ');
            while (index < fileInfo.Length)
            {
                fileInfo[index] = fileLine.Substring(0, position);
                fileLine = fileLine.Substring(position);
                fileLine = fileLine.Trim();
                index++;
                position = fileLine.IndexOf(' ');
            }
            serverData.fileName = fileLine;
            serverData.permission = fileInfo[0];
            serverData.owner = fileInfo[2];
            serverData.group = fileInfo[3];
            try
            {
                serverData.size = Convert.ToInt32(fileInfo[4]);
            }
            catch
            {

            }

            serverData.date = fileInfo[5] + ' ' + fileInfo[6] + ' ' + fileInfo[7];
            serverData.isDirectory = serverData.permission[0] == 'd';

            return serverData;
        }

        public void DisConnect()
        {
            _ftpHandler.PrepareCommand(new FtpCommand("QUIT", 221));
            _ftpHandler.IsConnect = false;
            if (thread_hB != null && thread_hB.IsAlive)
            {
                thread_hB.Abort();
                thread_hB = null;
            }
        }

        public void DirUp()
        {
            _ftpHandler.PrepareCommand(new FtpCommand("CDUP", 250));
            _ftpHandler.PrepareCommand(new FtpCommand("PWD", 257));
        }

        public void DirChange(string fileName)
        {
            _ftpHandler.PrepareCommand(new FtpCommand("CWD", 250, fileName));
            _ftpHandler.PrepareCommand(new FtpCommand("PWD", 257));
        }

        public void DowLoadFile(string serverFileName, int fileSize, string workid,int offset=0)
        {
            if (!isThreadForTransfer)
                new Thread(new ParameterizedThreadStart(thread_DownFile))
                          .Start(new Tuple<string, int, string,int >(serverFileName, fileSize, workid,offset));
            else
            {
                _ftpHandler.PrepareCommand(new FtpCommand("TYPE I", 200));
                _ftpHandler.PrepareCommand(new FtpCommand("RETR", 125, serverFileName, fileSize, workid,offset ));
            }
        }


        /// <summary>  
        /// 创建固定大小的临时文件  
        /// </summary>  
        /// <param name="fileName">文件名</param>  
        /// <param name="fileSize">文件大小</param>  
        /// <param name="overwrite">允许覆写：可以覆盖掉已经存在的文件</param>  
        public static void CreateFixedSizeFile(string fileName, long fileSize)
        {
            //验证参数  
            if (string.IsNullOrEmpty(fileName) || new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }.Contains(
                    fileName[fileName.Length - 1]))
                throw new ArgumentException("fileName");
            if (fileSize < 0) throw new ArgumentException("fileSize");

            //创建目录  
            string dir = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            //创建文件  
            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Create);
                fs.SetLength(fileSize); //设置文件大小  
            }
            catch
            {
                if (fs != null)
                {
                    fs.Close();
                    File.Delete(fileName); //注意，若由fs.SetLength方法产生了异常，同样会执行删除命令，请慎用overwrite:true参数，或者修改删除文件代码。  
                }
                throw;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }
        /// <summary>
        /// 下载线程
        /// </summary>
        /// <param name="param"></param>
        private void thread_DownFile(object param)
        {
            var p = (Tuple<string, int, string,int >)param;
            string serverFileName = p.Item1;
            int fileSize = p.Item2;
            string workid = p.Item3;
            int offset = p.Item4;

            string path = this.Localfolder;
            //CreateFixedSizeFile(path + @"\" + serverFileName + ".tmp", fileSize);

            var waits = new List<EventWaitHandle>();
            FtpHelper downftp = null;
            var handler = new ManualResetEvent(false);
            waits.Add(handler);
            downftp = new FtpHelper();
            downftp.Hostname = this.Hostname;
            downftp.Username = this.Username;
            downftp.Password = this.Password;
            downftp.Port = this.Port;

            downftp.DelEvent = this.DelEvent;
            downftp.isThreadForTransfer = true;
            downftp.Remotefolder = this.Remotefolder;
            downftp.Localfolder = this.Localfolder;
            downftp.Connect();

            downftp.Login();
            downftp._handler = handler;
            downftp.DirChange(downftp.Remotefolder);
            downftp.DowLoadFile(serverFileName, fileSize, workid,offset );

            WaitHandle.WaitAll(waits.ToArray());

            FtpEventArgs e = new FtpEventArgs();
            e.Totalbytes = fileSize;
            //e.Timeelapsed = timeElapsed;
            //e.SMessage = s;
            e.Workid = p.Item3;
            OnFileDownLoadCommpleted(e);
        }

        private void threads_down() { }



        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="localFileName"></param>
        /// <param name="fileSize"></param>
        /// <param name="workid"></param>
        public void UpLoadFile(string localFileName, int fileSize, string workid)
        {
            //是否是线程
            if (!isThreadForTransfer)
                //启动线程
                new Thread(new ParameterizedThreadStart(thread_UpLoad))
                            .Start(new Tuple<string, int, string>(localFileName, fileSize, workid));
            else
            {
                //直接上传
                _ftpHandler.PrepareCommand(new FtpCommand("TYPE I", 200));
                _ftpHandler.PrepareCommand(new FtpCommand("STOR", 226, localFileName, fileSize, workid));
            }
        }

        private void thread_UpLoad(object param)
        {
            var p = (Tuple<string, int, string>)param;
            string localFileName = p.Item1;
            int fileSize = p.Item2;
            string workid = p.Item3;

            var waits = new List<EventWaitHandle>();
            FtpHelper uploadftp = null;


            var handler = new ManualResetEvent(false);
            waits.Add(handler);

            uploadftp = new FtpHelper();
            uploadftp.Hostname = this.Hostname;
            uploadftp.Username = this.Username;
            uploadftp.Password = this.Password;
            uploadftp.Port = this.Port;

            uploadftp.DelEvent = this.DelEvent;

            uploadftp.isThreadForTransfer = true;
            uploadftp.Remotefolder = this.Remotefolder;
            uploadftp.Localfolder = this.Localfolder;
            uploadftp.Connect();

            uploadftp.Login();

            uploadftp._handler = handler;
            uploadftp.DirChange(uploadftp.Remotefolder );
            uploadftp.UpLoadFile(localFileName,fileSize ,workid );
     
            WaitHandle.WaitAll(waits.ToArray());

            FtpEventArgs e = new FtpEventArgs();
            e.Totalbytes = fileSize;
            //e.Timeelapsed = timeElapsed;
            //e.SMessage = s;
            e.Workid = workid;
            OnFileUpLoadCommpleted (e);
        }

        private Thread thread_hB = null;
        public  void HeartBeat()
        {
            if (thread_hB == null || !thread_hB .IsAlive)
            {
                thread_hB = new Thread(new ThreadStart(thread_HeartBeat));
                thread_hB.Start();
            }
        }
        private void thread_HeartBeat()
        {
            while (!isThreadForTransfer )
            {
                try
                {
                    this.WaitSingle();
                    Thread.Sleep(20000);
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void MakeFolder(string folderName)
        {
            _ftpHandler.PrepareCommand(new FtpCommand("MKD",257, folderName));
        }
        public void Abor()
        {
            _ftpHandler.PrepareCommand(new FtpCommand("ABOR", 226));
        }

        public void ClearCommandList()
        {
            _ftpHandler.ClearCommandList();
        }

        public void DeleteFile(string fileName)
        { 
           _ftpHandler.PrepareCommand(new FtpCommand("DELE",250,fileName));
        }

        public void DeleteFolder(string folderUrl)
        { 
           _ftpHandler.PrepareCommand(new FtpCommand("RMD",250,folderUrl));
        }

        public void WaitSingle()
        {
            if (!_ftpHandler.JudgetCommandList())
            {
                _ftpHandler.PrepareCommand(new FtpCommand("NOOP", 200));
            }
        }

        public void RnameFile(string originalFilePath, string newFilePath)
        {
            _ftpHandler.PrepareCommand(new FtpCommand("RNFR", 350,originalFilePath));
            _ftpHandler.PrepareCommand(new FtpCommand("RNTO",250,newFilePath));
        }
    }
}
