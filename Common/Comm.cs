using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;

namespace Common
{
    public class Comm
    {
        public static byte[] GetBytes(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        public static string GetString(byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer);
        }

        public static string GetString(byte[] buffer,int index,int count)
        {
            return Encoding.UTF8.GetString(buffer, index, count);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }
        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;

        [DllImport("shell32.dll")]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        public static void ShowFileProperties(string Filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = Filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            ShellExecuteEx(ref info);
        }

        public static void ShowErrorDialog(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void CallSysInfoDiaLog()
        {

            const string cRegKeySysInfo = @"SOFTWARE\Microsoft\Shared Tools\MSINFO";

            const string cRegValSysInfo = "PATH";

            const string cRegKeySysInfoLoc = @"SOFTWARE\Microsoft\Shared Tools Location";

            const string cRegValSysInfoLoc = "MSINFO";

            try
            {

                string fileName;


                //不同版本的windows系统，注册表存放的位置不同，共有两处


                //第一处

                RegistryKey getRegKey = Registry.LocalMachine;

                RegistryKey getSubKey = getRegKey.OpenSubKey(cRegKeySysInfo);



                if (getSubKey == null)
                {

                    //第二处

                    getSubKey = getRegKey.OpenSubKey(cRegKeySysInfoLoc);

                    fileName = getSubKey.GetValue(cRegValSysInfoLoc).ToString() + @"\MSINFO32.EXE";

                }

                else
                {

                    fileName = getSubKey.GetValue(cRegValSysInfo).ToString();

                }


                //调用外部可执行程序

                Process newPro = new Process();

                newPro.StartInfo.FileName = fileName;

                newPro.Start();

            }

            catch { }

        }

    }
}
