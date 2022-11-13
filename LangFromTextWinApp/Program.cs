using LangFromTextWinApp.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LangFromTextWinApp
{
    static class Program
    {
        static Mutex mutex = new Mutex(false, FEConstants.PRODUCT_NAME_Unique);

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        public static void Main(string[] args)
        {
            if (mutex.WaitOne(0))
            {
                try
                {
                    FEContext.IsAutorun = (string.Compare(args?.FirstOrDefault(), "-autorun", true) == 0);

                    App app = new App();
                    app.InitializeComponent();
                    app.Run();
                }
                finally { mutex.ReleaseMutex(); }
            }
            else
            {
                try
                {
                    NamedPipeTools.ClientSendData();
                }
                catch (Exception ex) { System.Windows.MessageBox.Show($"{FEConstants.PRODUCT_NAME} is already runnig. Please open app from tray icon menu.", FEConstants.PRODUCT_NAME, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning); }
            }
        }
    }
}
