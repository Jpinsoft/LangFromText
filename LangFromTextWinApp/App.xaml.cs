using Jpinsoft.LangTainer.Types;
using Jpinsoft.LangTainer.Utils;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace LangFromTextWinApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            switch (e.Exception)
            {
                case InfoException infoException:
                    MessageBoxWPF.ShowInfo(Application.Current.MainWindow, MessageBoxButton.OK, infoException.Message);
                    break;

                default:

#if DEBUG
                    MessageBoxWPF.ShowError(Application.Current.MainWindow, MessageBoxButton.OK, ExceptionUtils.AddInnerMessages(e.Exception) + e.Exception.StackTrace);
#else
                    MessageBoxWPF.ShowError(Application.Current.MainWindow, MessageBoxButton.OK, ExceptionUtils.AddInnerMessages(e.Exception));
#endif


                    break;
            }

        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            WPFHelpers.CheckForSettingsUpgrade();

            // Napr. pri autorun nie je CurrentDirectory nasetovane..
            string appDir = Path.GetDirectoryName((System.Reflection.Assembly.GetExecutingAssembly().Location));
            Directory.SetCurrentDirectory(appDir);

            WPFHelpers.SetLang(Settings.Default.AppLanguage);

            WPFHelpers.SetTheme(Settings.Default.CurrentTheme, false);
        }
    }
}
