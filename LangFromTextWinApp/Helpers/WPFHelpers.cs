using Jpinsoft.LangTainer.Utils;
using LangFromTextWinApp.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LangFromTextWinApp.Helpers
{
    public static class WPFHelpers
    {
        const int GWL_EXSTYLE = -20;
        const int WS_EX_DLGMODALFRAME = 0x0001;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_FRAMECHANGED = 0x0020;

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
                   int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hwnd, uint msg,
                   IntPtr wParam, IntPtr lParam);

        public static ImageSource ImageToBitmapImage(Bitmap image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, image.RawFormat);
            ms.Seek(0, SeekOrigin.Begin);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();
            return bi;
        }

        public static ImageSource IconToBitmapImage(Icon image)
        {
            return Imaging.CreateBitmapSourceFromHIcon(image.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public static Task BeginAsync(this Storyboard storyboard)
        {
            System.Threading.Tasks.TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            if (storyboard == null)
                tcs.SetException(new ArgumentNullException());
            else
            {
                EventHandler onComplete = null;

                onComplete = (s, e) =>
                {
                    storyboard.Completed -= onComplete;
                    tcs.SetResult(true);
                };

                storyboard.Completed += onComplete;
                storyboard.Begin();
            }
            return tcs.Task;
        }

        public static void SetLang(string langName)
        {
            if (!string.IsNullOrEmpty(langName))
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(langName);
            }

            Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        public static void SetTheme(string themeName, bool throwExOnError = true)
        {
            try
            {
                ResourceDictionary skin = new ResourceDictionary();
                skin.Source = new Uri("Themes/" + themeName + ".xaml", UriKind.RelativeOrAbsolute);
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(skin);
            }
            catch
            {
                if (throwExOnError)
                    throw new Exception($"Unable to set Theme - {themeName}");
            }
        }

        public static void OpenTranslator(string word)
        {
            try
            {
                if (string.IsNullOrEmpty(Settings.Default.URLTemplateOpenTranslator) || !Settings.Default.URLTemplateOpenTranslator.Contains(FEConstants.PLACEHOLDER_WORD))
                    throw new UriFormatException($"{Resources.T088} is not correct. PLease chceck Settings.");

                string openURL = Properties.Settings.Default.URLTemplateOpenTranslator.Replace(FEConstants.PLACEHOLDER_WORD, word);

                Process.Start(openURL);
            }
            catch (Exception ex)
            {
                MessageBoxWPF.ShowError(Application.Current.MainWindow, MessageBoxButton.OK, ExceptionUtils.AddInnerMessages(ex));
            }
        }

        public static void PlaceWindowToScreen(System.Windows.Window w)
        {
            if (w.WindowState != WindowState.Minimized)
            {
                if (w.Top < SystemParameters.VirtualScreenTop)
                    w.Top = SystemParameters.VirtualScreenTop;

                if (w.Left < SystemParameters.VirtualScreenLeft)
                    w.Left = SystemParameters.VirtualScreenLeft;

                if (w.Left > SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth)
                    w.Left = SystemParameters.VirtualScreenWidth + SystemParameters.VirtualScreenLeft - w.Width;

                if (w.Top > SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
                    w.Top = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - w.Height;
            }
        }

        public static void CheckForSettingsUpgrade()
        {
            if (Properties.Settings.Default.SettingsUpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.SettingsUpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
        }
    }
}
