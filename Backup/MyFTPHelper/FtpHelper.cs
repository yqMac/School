using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MyFTPHelper
{
    public delegate void FtpEventHandler(object sender,FtpEventArgs e);

    public partial class FtpHelper : Component
    {
        FtpHandler _ftpHandler;
        private string _username;						// username account
        private string _password;						// username password
        private string _hostname;						// hostname
        private int _port;								// host port

        private string _localfolder;					// local folder string path
        private string _remotefolder;					// remote folder string path

        private string _type;

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

        public event FtpEventHandler Error;
        public event FtpEventHandler Message;
        public event FtpEventHandler ConnectCommpleted;
        public event FtpEventHandler LoginFailure;
        public event FtpEventHandler LoginCommpleted;
        public event FtpEventHandler CommandCommpleted;
        public event FtpEventHandler BaseInfor;
        public event FtpEventHandler DirCommpleted;
        public event FtpEventHandler FileDownLoadCommpleted;
        public event FtpEventHandler FileUpLoadCommpleted;
        public event FtpEventHandler FilePassProgressCommpleted;
        public event FtpEventHandler ResponseUnexpected;

        public void OnResponseUnexpected(FtpEventArgs e)
        {
            if (ResponseUnexpected != null)
            {
                ResponseUnexpected(this, e);
            }
        }
        public void OnFilePassProgressCommpleted(FtpEventArgs e)
        {
            if (FileDownLoadCommpleted != null)
            {
                FilePassProgressCommpleted(this, e);
            }
        }
        public void OnFileDownLoadCommpleted(FtpEventArgs e)
        {
            if (FileDownLoadCommpleted != null)
            {
                FileDownLoadCommpleted(this, e);
            }
        }
        public void OnFileUpLoadCommpleted(FtpEventArgs e)
        {
            if (FileUpLoadCommpleted != null)
            {
                FileUpLoadCommpleted(this, e);
            }
        }
        public void OnMessage(FtpEventArgs e)
        {
            if (Message != null)
            {
                Message(this, e);
            }
        }
        public void OnConnectCommpleted(FtpEventArgs e)
        {
            if (ConnectCommpleted != null)
            {
                ConnectCommpleted(this, e);
            }
        }
        public void OnError(FtpEventArgs e)
        {
            if (Error != null)
            {
                Error(this,e);
            }
        }
        public void OnLoginFailure(FtpEventArgs e)
        {
            if (LoginFailure != null)
            {
                LoginFailure(this, e);
            }
        }
        public void OnLoginCommpleted(FtpEventArgs e)
        {
            if (LoginCommpleted != null)
            {
                LoginCommpleted(this, e);
            }
        }
        public void OnCommandCommpleted(FtpEventArgs e)
        {
            if (CommandCommpleted != null)
            {
                CommandCommpleted(this, e);
            }
        }
        public void OnBaseInfor(FtpEventArgs e)
        {
            if (BaseInfor != null)
            {
                BaseInfor(this, e);
            }
        }
        public void OnDirCommpleted(FtpEventArgs e)
        {
            if (DirCommpleted != null)
            {
                ParseDirList(e.SMessage);
                DirCommpleted(this, e);
            }
        }

        #endregion

        public void Login()
        {
            if (_username != string.Empty && _password != String.Empty)
            {
                _ftpHandler.PrepareCommand(new FtpCommand("USER", 331, _username));
                _ftpHandler.PrepareCommand(new FtpCommand("PASS", 230, _password));
                _ftpHandler.PrepareCommand(new FtpCommand("SYST",0));
                _ftpHandler.PrepareCommand(new FtpCommand("PWD", 257));
                _ftpHandler.PrepareCommand(new FtpCommand("TYPE", 200,"A"));
            }
        }

        public void Dir()
        { 
            _ftpHandler.PrepareCommand(new FtpCommand("TYPE I",200));
            _ftpHandler.PrepareCommand(new FtpCommand("LIST",226));
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
        }

        public void DirUp()
        {
            _ftpHandler.PrepareCommand(new FtpCommand("CDUP", 250));
            _ftpHandler.PrepareCommand(new FtpCommand("PWD",257));
        }

        public void DirChange(string fileName)
        { 
            _ftpHandler.PrepareCommand(new FtpCommand("CWD", 250, fileName));
            _ftpHandler.PrepareCommand(new FtpCommand("PWD", 257));
        }

        public void DowLoadFile(string serverFileName,int fileSize)
        {
            _ftpHandler.PrepareCommand(new FtpCommand("TYPE I", 200));
            _ftpHandler.PrepareCommand(new FtpCommand("RETR", 125,serverFileName,fileSize));
        }

        public void UpLoadFile(string localFileName, int fileSize)
        {
            _ftpHandler.PrepareCommand(new FtpCommand("TYPE I", 200));
            _ftpHandler.PrepareCommand(new FtpCommand("STOR", 226, localFileName, fileSize));
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
