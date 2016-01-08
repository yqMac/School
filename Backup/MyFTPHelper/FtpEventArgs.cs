using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyFTPHelper
{
    public class FtpEventArgs:EventArgs
    {
        private int m_areadyBytes;

        public int AreadyBytes
        {
            get { return m_areadyBytes; }
            set { m_areadyBytes = value; }
        }

        private string m_sMessage;

        public string SMessage
        {
            get { return m_sMessage; }
            set { m_sMessage = value; }
        }
        private int m_timeelapsed;			// Time elapsed

        public int Timeelapsed
        {
            get { return m_timeelapsed; }
            set { m_timeelapsed = value; }
        }
        private int m_totalbytes;			// Total Bytes

        public int Totalbytes
        {
            get { return m_totalbytes; }
            set { m_totalbytes = value; }
        }
        private string m_function;

        public string Function
        {
            get { return m_function; }
            set { m_function = value; }
        }

        public FtpEventArgs()
        {
            m_sMessage = string.Empty;
            m_function = string.Empty;
            m_totalbytes = 0;
            m_timeelapsed = 0;
        }

        public FtpEventArgs(string sMessage)
        {
            m_sMessage = sMessage;
        }
    }
}
