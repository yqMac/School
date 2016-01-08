using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Common;


namespace MyFTPHelper
{
    /// <summary>
    /// 每个数据传输对象
    /// </summary>
    public class DataStataObject
    {
        public Socket workSocket=null;
        public const int BUFFER_SIZE = 10240;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public StringBuilder sb = new StringBuilder();
        public FileStream fs = null;
        public int byteTransfered = 0;
        public byte[] bufferUpLoad;
        public FtpHandler ftpHandler;
        public int byteRead;
    }

   /// <summary>
   /// 数据传输的Socket对象
   /// </summary>
    public class DataAsyncSocket
    {
        private string _lastError;
        private int _cmdData;
        private Socket _socket;
        private string _fileName;
        private int _fileSize;
        private FtpHandler _ftpHandler;
        private string _response;
        private const string CRLF = "\r\n";
        private int _startTick;
        private int _endTick;
        private string  _workid;

        public DataAsyncSocket(FtpHandler ftpHandler)
        {
            _lastError = String.Empty;
            _cmdData = 0;
            _ftpHandler = ftpHandler;
            _response = String.Empty;
        }

        /// <summary>
        /// 链接数据传输ip port,保存当前工作id
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        /// <param name="workid"></param>
        public void Connect(string hostName,int port,string workid)
        {
            IPAddress ipAdress=IPAddress.Parse(hostName);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAdress, port);
            this._workid  = workid;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
           
            _socket.BeginConnect(ipEndPoint, new AsyncCallback(ConnectionCallBack), _socket);
        }

        /// <summary>
        /// 链接完毕回调
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectionCallBack(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;
            sock.EndConnect(ar);
            if (_cmdData == 0)
            {
                Recive();
            }
            else
            {
                if (_cmdData == 1)
                {
                    ReciveFile();
                }
                else
                {
                    SendFile();
                }
            }
            _ftpHandler.ExcuteNextCommand();
        }

        /// <summary>
        /// 数据传输——接受信息
        /// </summary>
        public void Recive()
        {
            _startTick = Environment.TickCount;

            DataStataObject state = new DataStataObject();
            state.workSocket = _socket;

            _socket.BeginReceive(state.buffer, 0, DataStataObject.BUFFER_SIZE,
                0, new AsyncCallback(ReciveData), state);
        }

        /// <summary>
        /// 数据传输--Recive回调
        /// </summary>
        /// <param name="ar"></param>
        public void ReciveData(IAsyncResult ar)
        {
            DataStataObject state = (DataStataObject)ar.AsyncState;
            Socket sock = state.workSocket;

            int byteRead = sock.EndReceive(ar);
            state.byteTransfered += byteRead;

            if (byteRead > 0)
            {
                string s = Comm.GetString(state.buffer, 0, byteRead);
                state.sb.Append(s);
                if (state.sb.ToString().EndsWith(CRLF))
                {
                    _endTick = Environment.TickCount;
                    _response = state.sb.ToString();
                    _ftpHandler.TransferCommpleted(state.byteTransfered, _endTick - _startTick,this );
                }
                else
                {
                    sock.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReciveData), state);
                }
            }
            else 
            {
                _endTick = Environment.TickCount;
                _ftpHandler.TransferCommpleted(state.byteTransfered, _endTick - _startTick, this);
            }
        }

        /// <summary>
        /// 发送文件
        /// </summary>
        public void SendFile()
        {
            DataStataObject state = new DataStataObject();
            try
            {
                _startTick = Environment.TickCount;
                
                state.workSocket = _socket;
                state.fs = new FileStream(_fileName, FileMode.Open);
                state.byteTransfered = 0;
                _fileSize = (int)state.fs.Length;

                state.bufferUpLoad = new byte[1024];

                if (_fileSize >= 1024)
                {
                    state.fs.Read(state.bufferUpLoad, 0, state.bufferUpLoad.Length);
                    _socket.BeginSend(state.bufferUpLoad, 0, state.bufferUpLoad.Length,
                                       0, new AsyncCallback(SendFileCallBack), state);
                }
                else
                {
                    state.fs.Read(state.bufferUpLoad, 0, _fileSize);
                    _socket.BeginSend(state.bufferUpLoad, 0, _fileSize,
                                      0, new AsyncCallback(SendFileCallBack), state);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 数据传输--发送文件回调
        /// </summary>
        /// <param name="ar"></param>
        public void SendFileCallBack(IAsyncResult ar)
        {
            DataStataObject state = (DataStataObject)ar.AsyncState;
            Socket sock = state.workSocket;

            try
            {
               
                int byteSend = sock.EndSend(ar);
                state.byteTransfered += byteSend;

                _endTick = Environment.TickCount;
                _ftpHandler.ShowFilePassProgress(_fileName, state.byteTransfered, _fileSize, _endTick - _startTick);
                if (state.byteTransfered >= _fileSize || byteSend <=0)
                {
                    sock.Close();
                    state.fs.Close();
                    _endTick = Environment.TickCount;
                    _ftpHandler.TransferCommpleted(state.byteTransfered, _endTick - _startTick, this);
                }
                else
                {
                    if (_fileSize - state.byteTransfered > 1024)
                    {
                        state.fs.Read(state.bufferUpLoad,0 , state.bufferUpLoad.Length);
                        _socket.BeginSend(state.bufferUpLoad, 0,
                        state.bufferUpLoad.Length, 0, new AsyncCallback(SendFileCallBack), state);
                    }
                    else
                    {
                        state.fs.Read(state.bufferUpLoad, 0, _fileSize - state.byteTransfered);
                        _socket.BeginSend(state.bufferUpLoad, 0,
                        _fileSize - state.byteTransfered, 0, new AsyncCallback(SendFileCallBack), state);
                    }
                }
            }
            catch (Exception ex)
            {
                sock.Close();
                state.fs.Close();
                _ftpHandler.ErrorNotify(ex.Message, "DataAsyncSocket.SendFileCallBack");
            }
        }

        /// <summary>
        /// 数据传输--接受文件
        /// </summary>
        public void ReciveFile()
        {
            _startTick = Environment.TickCount;

            DataStataObject state = new DataStataObject();

            state.workSocket = _socket;
            state.fs = new FileStream(_fileName, FileMode.Create);
            state.byteTransfered = 0;

            _socket.BeginReceive(state.buffer, 0, StateObject.BufferSize,
                0, new AsyncCallback(ReciveFileCallBack), state);
        }

        /// <summary>
        /// 数据传输--接受文件回调
        /// </summary>
        /// <param name="ar"></param>
        public void ReciveFileCallBack(IAsyncResult ar)
        {
            DataStataObject state = (DataStataObject)ar.AsyncState;
            Socket sock = state.workSocket;

            int byteRead = sock.EndReceive(ar);
            
            state.fs.Write(state.buffer, 0, byteRead);
            state.byteTransfered += byteRead;

            _endTick = Environment.TickCount;
            _ftpHandler.ShowFilePassProgress(_fileName,state.byteTransfered, _fileSize, _endTick - _startTick);
            if (state.byteTransfered >= _fileSize || byteRead <=0)
            {
                _endTick = Environment.TickCount;
          
                _ftpHandler.TransferCommpleted(state.byteTransfered, _endTick - _startTick, this);
                sock.Close();
                state.fs.Close();
            }
            else
            {
                sock.BeginReceive(state.buffer, 0, StateObject.BufferSize, 
                    0, new AsyncCallback(ReciveFileCallBack), state);
            }
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisConnect()
        {
            _socket.Close();
        }

        public string Response
        {
            get { return _response; }
        }

        /// <summary>
        /// 标志 传输类型、list上传下载
        /// </summary>
        public int CmdData
        {
            get { return _cmdData; }
            set { _cmdData = value; }
        }

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public int FileSize
        {
            get { return _fileSize; }
            set { _fileSize = value; }
        }

        public string  Workid
        {
            get
            {
                return _workid;
            }

            set
            {
                _workid = value;
            }
        }
    }
}
