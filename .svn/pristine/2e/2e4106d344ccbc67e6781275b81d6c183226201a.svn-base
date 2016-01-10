using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyFTPHelper
{
    public class FtpHandler
    {
         private FtpSocket _asyncSocket;
         //private DataAsyncSocket _dataAsyncSocket;
         private FtpHelper _ftpHelper;
         private bool _isConnect = false;

         public bool IsConnect
         {
             get { return _isConnect; }
             set { _isConnect = value; }
         }
         private FtpCommand _currentCommand;
         private List<FtpCommand> _cmdList = new List<FtpCommand>();
         private FtpCommand _nextCommand;
         

         public FtpHandler(FtpHelper ftpHelper)
         {
             _ftpHelper = ftpHelper;
             _asyncSocket = null;
         }

         public bool Connect(string host, int port)
         {
             bool bres = false;
             if (_isConnect)
                 return false;
             if (host != "" && port != 0)
             {
                 _asyncSocket = new FtpSocket(this);
                 _asyncSocket.Connect(host, port);
                 bres = true;
             }
             return bres;
         }

        /// <summary>
        /// 处理响应信息
        /// </summary>
        /// <param name="response"></param>
        /// <param name="state"></param>
         public void Reply(string response,StateObject state)
         {
            //显示信息
             MessageNotify(response);

            //响应代码
             string sCode = response.Substring(0, 3);

             
             int iCode = 0;

             try
             {
                 iCode = Convert.ToInt32(sCode);
             }
             catch
             {
                 return;
             }


             if (iCode >= 100 && iCode < 200)
                 
             return;
             //if (iCode == 125||iCode==150)
             //    return;
             if (_currentCommand == null)
             {
                 if (iCode == 220)
                     ConnectionCommpleted();
                 if (iCode == 421)
                     ConnectionTerminated();
             }
             else 
             {
                //返回状态码与设定正确状态码匹配，说明当前命令正确执行
                 if (_currentCommand.Response == iCode)
                 {
                    //当前在请求数据链接
                     if (_currentCommand.Command == "PASV")
                     {
                         string IpAdress=String.Empty;
                         int port=0;
                        //字符串处理，返回ip、port
                         RetrieveDataPort(response,ref IpAdress,ref port);
                        //创建数据传输Socket
                         CreateDataSocket(IpAdress, port );
             
                    }
                     else
                     {

                         if (_currentCommand.Command == "QUIT")
                         {
                             DisConnect();
                         }
                         if (_currentCommand.Command == "PASS")
                         {
                             LoginCommpeleted();
                         }
                         if (_currentCommand.Command == "PWD")
                         {
                             int firstPos = response.IndexOf('"');
                             int lastPos = response.LastIndexOf('"');

                             string remoteFolder = response.Substring(firstPos+1,lastPos-firstPos-1);
                             _ftpHelper.Remotefolder = remoteFolder;
                         }
                         ExcuteNextCommand();
                     }
                 }
                 else
                 {
                     if (_currentCommand.Response != 0)
                     {
                         if (_currentCommand.Command == "PASS" && iCode == 530)
                         {
                             LoginFailure();
                         }
                         else
                         {

                             string s = _currentCommand.Command + " " + _currentCommand.Response.ToString() + " " + sCode;
                             ResponseUnexpected(s+"\r\nServer response different that expected"+"\n"+response);
                    
                             ExcuteNextCommand();
                         }
                     }
                     else 
                     {
                         ExcuteNextCommand();
                     }
                 }
             }

         }

         #region Private Method

        /// <summary>
        /// 根据下一跳命令，创建list、上传、下载3中不同的功能的DATASocket
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
         private void CreateDataSocket(string ipAddress, int port)
         {
            string workid =  "";
            DataAsyncSocket _dataAsyncSocket = new DataAsyncSocket(this);
             if (_nextCommand.Command == "LIST")
             {
                 _dataAsyncSocket.CmdData = 0;
             }
             else
            {
                workid = _nextCommand.WorkId;
                if (_ftpHelper.Localfolder.EndsWith("\\"))
                 {
                     _dataAsyncSocket.FileName = _ftpHelper.Localfolder + _nextCommand.SParam;
                 }
                 else 
                 {
                     _dataAsyncSocket.FileName = _ftpHelper.Localfolder + "\\" + _nextCommand.SParam;
                 }
                 if (_nextCommand.Command == "RETR")
                 {
                     _dataAsyncSocket.FileSize = _nextCommand.IParam;
                     _dataAsyncSocket.CmdData = 1;
                   
                 }
                 else if(_nextCommand.Command=="STOR")
                 {
                     _dataAsyncSocket.FileSize = _nextCommand.IParam;
                     _dataAsyncSocket.CmdData = 2;
                 }
             }
             //启动链接等待传输
             _dataAsyncSocket.Connect(ipAddress, port,workid  );
         }

         private void RetrieveDataPort(string response,ref string IPAdress,ref int port)
         {
             int firsPos,finaPas;
             string[] adressIPKit;
             string responseIp = "";
             firsPos = response.IndexOf('(')+1;
             finaPas = response.IndexOf(')');
             responseIp = response.Substring(firsPos, finaPas - firsPos);
             adressIPKit = responseIp.Split(',');

             for (int i = 0; i < 3; i++)
             {
                 IPAdress += adressIPKit[i];
                 IPAdress += ".";
             }
             IPAdress += adressIPKit[3];
             port = Convert.ToInt32(adressIPKit[4]) * 256 + Convert.ToInt32(adressIPKit[5]);
         }

        /// <summary>
        /// 数据传输完毕的回调函数
        /// </summary>
        /// <param name="byteTransfered"></param>
        /// <param name="timeElapsed"></param>
        /// <param name="_dataAsyncSocket"></param>
         public void TransferCommpleted(int byteTransfered, int timeElapsed,DataAsyncSocket _dataAsyncSocket  ) 
         {
             string s;
             if (_dataAsyncSocket == null)
             {
                 s = String.Empty;
             }
             else
             {
                 s = _dataAsyncSocket.Response;
                 _dataAsyncSocket.DisConnect();
             }
            
             FtpEventArgs e = new FtpEventArgs();
             e.Totalbytes = byteTransfered;
             e.Timeelapsed = timeElapsed;
             e.SMessage = s;
             e.Workid = _dataAsyncSocket.Workid;
       
             if (_dataAsyncSocket.CmdData == 0)
                 _ftpHelper.OnDirCommpleted(e);
             else if (_dataAsyncSocket.CmdData == 1)
                 _ftpHelper.OnFileDownLoadCommpleted(e);
             else
                 _ftpHelper.OnFileUpLoadCommpleted(e);

             
             _dataAsyncSocket = null;
            
         }

        /// <summary>
        /// 添加命令到执行列表
        /// </summary>
        /// <param name="command"></param>
         private void AddCommandList(FtpCommand command)
         {
             if (_nextCommand == null)
             {
                 _nextCommand = command;
             }
             _cmdList.Add(command);
         }

        /// <summary>
        /// 准备命令，适宜的添加到执行列表
        /// </summary>
        /// <param name="command"></param>
         public void PrepareCommand(FtpCommand command)
         {
             if (_currentCommand != null || _isConnect == false)
             {
                 if (command.Command == "RETR" || command.Command == "STOR" || command.Command == "LIST")
                 {
                     AddCommandList(new FtpCommand("PASV", 227));// add pasv
                    //有偏移说明是断点下载
                    if(command .Offset != 0)
                    {
                        AddCommandList(new FtpCommand("REST",350,command.Offset .ToString ()));
                    }
                 }
                 AddCommandList(command);
             }
             else
             {
                 if (command.Command == "RETR" || command.Command == "STOR" || command.Command == "LIST")
                 {
                    //有偏移说明是断点下载
                    if (command.Offset != 0)
                    {
                        AddCommandList(new FtpCommand("REST", 350, command.Offset.ToString()));
                    }
                    AddCommandList(command);
                     ExcuteCommand(new FtpCommand("PASV", 227));//add pasv and send the pasv request
                 }
                 else
                 {
                     ExcuteCommand(command);
                 }

             }
         }

        /// <summary>
        /// 执行指定命令
        /// </summary>
        /// <param name="command"></param>
         public  void ExcuteCommand(FtpCommand command)
         {
             _currentCommand = command;
             if (_cmdList.Count > 0)
             {
                 _nextCommand = _cmdList[0];
             }
             else 
             {
                 _nextCommand = null;
             }

             CommandNotify(command.GetCommandLine());
             _asyncSocket.Send(command.GetCommandLine());
         }

        /// <summary>
        /// 执行下一条命令
        /// </summary>
         public void ExcuteNextCommand()
         {
             if (_cmdList.Count > 0)
             {
                 FtpCommand cmd = _cmdList[0];
                 _cmdList.RemoveAt(0);
                 ExcuteCommand(cmd);
             }
             else
             {
                 _currentCommand = null;
             }
         }

        #region 事件调用的回调

        public void ErrorNotify(string message,string functionName)
         {
             FtpEventArgs e = new FtpEventArgs();
             e.SMessage = message;
             e.Function = functionName;
             _ftpHelper.OnError(e);
         }

         private void CommandNotify(string message)
         {
             FtpEventArgs e = new FtpEventArgs();
             e.SMessage = message.Substring(0, message.Length - 2);//????????????
             _ftpHelper.OnCommandCommpleted(e);
         }

         private void ResponseUnexpected(string message)
         {
             FtpEventArgs e = new FtpEventArgs();
             e.SMessage = message.Substring(0, message.Length - 2);
             _ftpHelper.OnResponseUnexpected(e);
         }
         private void MessageNotify(string message)
         {
             FtpEventArgs e = new FtpEventArgs();
             e.SMessage = message.Substring(0, message.Length - 2);
             _ftpHelper.OnMessage(e);
         }

         private void LoginCommpeleted()
         {
             FtpEventArgs e = new FtpEventArgs();
             e.SMessage = String.Empty;
             _ftpHelper.OnLoginCommpleted(e);
         }

         private void LoginFailure()
         {
             CancleCommand();
             FtpEventArgs e = new FtpEventArgs();
             e.SMessage = String.Empty;
             _ftpHelper.OnLoginFailure(e);
         }

         private void CancleCommand()
         {
             _cmdList.Clear();
             _currentCommand = null;
         }

         private void ConnectionCommpleted()
         {
             FtpEventArgs eNew = new FtpEventArgs();
             eNew.SMessage = "Connecting " + _ftpHelper.Hostname + ",Port "
                 + _ftpHelper.Port + "\r\n" + "Connected,Waiting for response";
             _ftpHelper.OnBaseInfor(eNew);


             FtpEventArgs e = new FtpEventArgs();
             e.SMessage = String.Empty;
             _ftpHelper.OnConnectCommpleted(e);

             _isConnect = true;
             ExcuteNextCommand();
         }

        /// <summary>
        /// 连接失败
        /// </summary>
         private void ConnectionTerminated()
         {
             FtpEventArgs eNew = new FtpEventArgs();
             eNew.SMessage = "Connecting " + _ftpHelper.Hostname + ",Port "
                 + _ftpHelper.Port + "\r\n" + "DisConnected,The remote cannot reach!";
             _ftpHelper.OnBaseInfor(eNew);

             DisConnect();
         }

         public void DisConnect()
         {
             if (_isConnect)
             {
                 _asyncSocket.Close();
                 _asyncSocket = null;
                 _isConnect = false;
             }
         }
        #endregion 时间调用的回调

        #endregion

        /// <summary>
        /// 显示文件传输进度条
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="areadyBytes"></param>
        /// <param name="totleBytes"></param>
        /// <param name="timeElapsed"></param>
        public void ShowFilePassProgress(string fileName,int areadyBytes,int totleBytes,int timeElapsed)
         {   
             FtpEventArgs e = new FtpEventArgs();
             e.AreadyBytes = areadyBytes;
             e.Totalbytes = totleBytes;
             e.Timeelapsed = timeElapsed;
             if (fileName.Contains('\\'))
             {
                 int index = fileName.LastIndexOf('\\');
                 e.SMessage = fileName.Substring(index + 1);
             }
             else 
             {
                 e.SMessage = fileName;
             }
             _ftpHelper.OnFilePassProgressCommpleted(e);
         }

         public void ClearCommandList()
         {
             _cmdList.Clear();
         }

        /// <summary>
        /// 是否还有命令未执行
        /// </summary>
        /// <returns></returns>
         public bool JudgetCommandList()
         {
             if (_cmdList.Count > 0)
             {
                 return true;
             }
             else 
             {
                 return false;
             }
         }

    }
}
