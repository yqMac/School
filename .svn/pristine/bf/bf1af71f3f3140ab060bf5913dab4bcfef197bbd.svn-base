using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;


namespace MyFTPHelper
{
    public class StateObject
    {
        public Socket workSocket = null;              // Client socket.
        public const int BufferSize = 512;            // Size of receive buffer.
        public byte[] buffer = new byte[BufferSize];  // Receive buffer.
        public StringBuilder sb = new StringBuilder();// Received data string.
        public string scode = string.Empty;
        public StringBuilder multires = new StringBuilder();
    }

    public class AsyncSocket
    {
        #region Define date number
        private Socket _socket;
        private const string CRLF = "\r\n";
        FtpHandler _ftpHandler;
        #endregion

        public AsyncSocket(FtpHandler ftpHandler)
        {
            _ftpHandler=ftpHandler;
        }

        public void Connect(string host,int port)
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint
                        (IPAddress.Parse(host), port);

                _socket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(remoteEndPoint);
                //_socket.BeginConnect(remoteEndPoint, new AsyncCallback(ConnectionCallBack), _socket);

                Recive();
            }
            catch (FormatException)
            {
                _ftpHandler.ErrorNotify("The formate of the IpAddress is not correct", "AsyncSocket.Connect");
            }
            catch (Exception ex)
            {
                _ftpHandler.ErrorNotify(ex.Message, "AsyncSocket.Connect");
            }
        }

        public void ConnectionCallBack(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;
            sock.EndConnect(ar);
            Recive();
        }

        public void Send(string data)
        {
            try
            {
                byte[] bufferData = Common.Comm.GetBytes(data);
                _socket.Send(Common.Comm.GetBytes(data));
            }
            catch (Exception ex)
            {
                _ftpHandler.ErrorNotify(ex.Message, "AsyncSocket.Send");
            }
            //_socket.BeginSend(bufferData, 0, bufferData.Length, 0, 
               // new AsyncCallback(SendCallBack), _socket);
        }

        public void SendCallBack(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = sock.EndSend(ar);
        }

        private void Recive()
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = _socket;

                _socket.BeginReceive(state.buffer, 0, StateObject.BufferSize,
                    SocketFlags.None, new AsyncCallback(ReciveData), state);
            }
            catch (Exception ex)
            {
                _ftpHandler.ErrorNotify(ex.Message, "AsyncSocket.Recive");
            }
        }

        private void ReciveData(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket sock = state.workSocket;
                int byteRead = sock.EndReceive(ar);
                if (byteRead > 0)
                {
                    string s = Common.Comm.GetString(state.buffer, 0, byteRead);
                    s = s.Replace("\0", "");
                    state.sb.Append(s);
                    string response = state.sb.ToString();
                    if (response.EndsWith(CRLF))
                    {
                        //MessageBox.Show(state.sb.ToString());
                        ReciveResponse(response, state);

                        state.sb.Remove(0, state.sb.Length);
                    }
                }
                sock.BeginReceive(state.buffer, 0, StateObject.BufferSize,
                    SocketFlags.None, new AsyncCallback(ReciveData), state);
            }
            catch (ObjectDisposedException)
            {
                //Nothing
            }
            catch (Exception ex)
            {
                _ftpHandler.ErrorNotify(ex.Message, "AsyncSocket.ReciveData");
            }
        }

        private void ReciveResponse(string response, StateObject state)
        {
            string[] responseArray = response.Split('\r', '\n');

            List<string> responseList = new List<string>();
            for (int i = 0; i < responseArray.Length; i++)
            {
                if (responseArray[i] != String.Empty)
                {
                    responseList.Add(responseArray[i]);
                }
            }
            int idLine = 0;
            int idStart = 0;

            if (responseList[idLine][3] == '-' || state.multires.Length > 0)
            {
                if (state.multires.Length == 0)
                {
                    state.multires.Append(responseList[idLine]);
                    state.scode = responseList[idLine].Substring(0, 3);
                    idStart = idLine + 1;
                }
                else
                {
                    idStart = idLine;
                }
                int j;
                for ( j = idStart; j < responseList.Count; j++)
                {
                    state.multires.Append(responseList[j]);
                    if (responseList[j].Length > 3)
                    {
                        if (responseList[j].Substring(0, 3) == state.scode && responseList[j][3] != '-')
                            break;
                    }
                }
                if (j < responseList.Count)
                {
                    _ftpHandler.Reply(state.multires.ToString(),state);
                    state.multires.Remove(0, state.multires.Length);
                }
                idLine = j + 1;

            }
            else 
            {
                _ftpHandler.Reply(response, state);
            }
        }

        public void Close()
        {
            try 
            {
                _socket.Close();
            }
            catch(Exception ex)
            {
                _ftpHandler.ErrorNotify(ex.Message, "AsyncSocket.Close");
            }
        }

    }
}
