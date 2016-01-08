using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyFTPHelper
{
    public class ServerFileData
    {
        public bool isDirectory;
        public string fileName;
        public int size;
        public string type;
        public string date;
        public string permission;
        public string owner;
        public string group;

        public ServerFileData()
        {
        }
    }
}
