using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyFtpSoft
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        //[MTAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (AutoUpdateForm.AutoCheckUpdate() > 0)
            {
                Application.Run(new AutoUpdateForm());
            }
            else
            {
                Application.Run(new Main());
            }

            //Application.Run(new Form1());
        }
    }
}
