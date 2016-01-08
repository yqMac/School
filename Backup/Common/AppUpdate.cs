using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections;
using System.IO;
using System.Xml;

namespace Common
{
    public class AppUpdate
    {
        private string _updaterUrl;

        public string UpdaterUrl
        {
            set { _updaterUrl = value; }
            get { return this._updaterUrl; }
        }

        public void DownAutoUpdateFile(string downLoadPath)
        {
            if (!System.IO.Directory.Exists(downLoadPath))
                System.IO.Directory.CreateDirectory(downLoadPath);
            string serverXmlFile = downLoadPath + @"/UpdateList.xml";
            try
            {
                WebRequest request = WebRequest.Create(_updaterUrl);
                WebResponse response = request.GetResponse();
                if (response.ContentLength > 0)
                {
                    WebClient client = new WebClient();
                    client.DownloadFile(this._updaterUrl, serverXmlFile);
                }
            }
            catch 
            {
                return;
            }
        }

        public int CheckForUpdate(string serverXmlFile, string localXmlFile, out Hashtable updateFileList)
        {
            updateFileList = new Hashtable();
            if (!File.Exists(localXmlFile) || !File.Exists(serverXmlFile))
            {
                return -1;
            }

            XmlFiles serverXmlFiles = new XmlFiles(serverXmlFile);
            XmlFiles localXmlFiles = new XmlFiles(localXmlFile);

            XmlNodeList newNodeList = serverXmlFiles.GetNodeList("AutoUpdater/Files");
            XmlNodeList oldNodeList = localXmlFiles.GetNodeList("AutoUpdater/Files");//5+1+a+s+p+x

            int k = 0;
            for (int i = 0; i < newNodeList.Count; i++)
            {
                string[] fileList = new string[3];

                string newFileName = newNodeList.Item(i).Attributes["Name"].Value.Trim();
                string newVer = newNodeList.Item(i).Attributes["Ver"].Value.Trim();

                ArrayList oldFileAl = new ArrayList();
                for (int j = 0; j < oldNodeList.Count; j++)
                {
                    string oldFileName = oldNodeList.Item(j).Attributes["Name"].Value.Trim();
                    string oldVer = oldNodeList.Item(j).Attributes["Ver"].Value.Trim();

                    oldFileAl.Add(oldFileName);
                    oldFileAl.Add(oldVer);

                }
                int pos = oldFileAl.IndexOf(newFileName);
                if (pos == -1)
                {
                    fileList[0] = newFileName;
                    fileList[1] = newVer;
                    updateFileList.Add(k, fileList);
                    k++;
                }
                else if (pos > -1 && newVer.CompareTo(oldFileAl[pos + 1].ToString()) > 0)
                {
                    fileList[0] = newFileName;
                    fileList[1] = newVer;
                    updateFileList.Add(k, fileList);
                    k++;
                }

            }
            return k;
        }
    }
}
