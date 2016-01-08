using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MyFTPHelper
{
    public class FtpCommand
    {
        private string _command = String.Empty;// FTP Command (ex: USER, RETR, ....)

        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }

        private int _response = 0;// Code reponse attented

        public int Response
        {
            get { return _response; }
            set { _response = value; }
        }

        private int _iParam = 0;// Int parameter (ex : filesize)

        public int IParam
        {
            get { return _iParam; }
            set { _iParam = value; }
        }
        private string _sParam = "";// String parameter (ex : filename)

        public string SParam
        {
            get { return _sParam; }
            set { _sParam = value; }
        }

        public string WorkId
        {
            get
            {
                return _workId;
            }

            set
            {
                _workId = value;
            }
        }

        /// <summary>
        /// 文件传输的偏移
        /// </summary>
        public int Offset
        {
            get
            {
                return _offset;
            }

            set
            {
                _offset = value;
            }
        }

        private string _workId;

        private int _offset;

  

        private const string CRLF = "\r\n";

        public string GetCommandLine()
        {
            string str;
            str = _command;

            if (_sParam != "")
                str = str + " " + _sParam;
            str += CRLF;
            return str;
        }

        public FtpCommand(string cmd, int response)
        {
            _command = cmd;
            _response = response;
        }

        public FtpCommand(string cmd, int response, string sParam)
        {
            _command = cmd;
            _response = response;
            _sParam = sParam;
        }

        public FtpCommand(string cmd, int response, string sParam, int iParam)
        {
            _command = cmd;
            _response = response;
            _sParam = sParam;
            _iParam = iParam;
        }

        public FtpCommand(string cmd, int response, string sParam, int iParam,string workid)
        {
            _command = cmd;
            _response = response;
            _sParam = sParam;
            _iParam = iParam;
            _workId = workid;
        }

        
        public FtpCommand(string cmd, int response, string sParam, int iParam, string workid,int offset)
        {
            _command = cmd;
            _response = response;
            _sParam = sParam;
            _iParam = iParam;
            _workId = workid;
            _offset = offset;
        }
    }
}
