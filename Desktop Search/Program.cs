﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Desktop_Search
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //   [STAThread]
        //   static void Main()
        //   {
        //       Application.EnableVisualStyles();
        //       Application.SetCompatibleTextRenderingDefault(false);
        //       Application.Run(new Form1());
        //  }

        [STAThread]
        static void Main()
        {
         //  if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();
           Application.EnableVisualStyles();
           Application.SetCompatibleTextRenderingDefault(false);
           Application.Run(new Form1());             // Edit as needed
        }

    //    [System.Runtime.InteropServices.DllImport("user32.dll")]
   //     private static extern bool SetProcessDPIAware();
    }
}