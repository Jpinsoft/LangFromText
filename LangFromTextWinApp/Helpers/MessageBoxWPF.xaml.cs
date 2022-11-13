using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LangFromTextWinApp.Helpers
{
    /// <summary>
    /// Interaction logic for MessageBoxWPF.xaml
    /// </summary>
    public partial class MessageBoxWPF : Window
    {
        public static bool? ShowInfo(Window parrent, MessageBoxButton buttons, string text)
        {
            SystemSounds.Asterisk.Play();
            MessageBoxWPF msg = new MessageBoxWPF();
            msg.ShowDialog(parrent, buttons, Properties.Resources.T005, text, SystemIcons.Information);
            return msg.DialogResult;
        }

        public static bool? ShowInfoFormat(Window parrent, MessageBoxButton buttons, string format, params object[] args)
        {
            return MessageBoxWPF.ShowInfo(parrent, buttons, string.Format(format, args));
        }

        public static bool? ShowQuestion(Window parrent, MessageBoxButton buttons, string caption, string text)
        {
            SystemSounds.Asterisk.Play();
            MessageBoxWPF msg = new MessageBoxWPF();
            msg.ShowDialog(parrent, buttons, caption, text, SystemIcons.Question);

            return msg.DialogResult;
        }

        public static bool? ShowQuestion(Window parrent, MessageBoxButton buttons, string text)
        {
            return MessageBoxWPF.ShowQuestion(parrent, buttons, Properties.Resources.T008, text);
        }

        public static bool? ShowWarning(Window parrent, MessageBoxButton buttons, string text)
        {
            SystemSounds.Exclamation.Play();
            MessageBoxWPF msg = new MessageBoxWPF();
            msg.ShowDialog(parrent, buttons, Properties.Resources.T006, text, SystemIcons.Warning);

            return msg.DialogResult;
        }

        public static bool? ShowWarningFormat(Window parrent, MessageBoxButton buttons, string format, params object[] args)
        {
            return MessageBoxWPF.ShowWarning(parrent, buttons, string.Format(format, args));
        }

        public static bool? ShowError(Window parrent, MessageBoxButton buttons, string text)
        {
            SystemSounds.Hand.Play();
            MessageBoxWPF msg = new MessageBoxWPF();
            msg.ShowDialog(parrent, buttons, Properties.Resources.T007, text, SystemIcons.Error);

            return msg.DialogResult;
        }

        public static bool? ShowErrorFormat(Window parrent, MessageBoxButton buttons, string format, params object[] args)
        {
            return MessageBoxWPF.ShowError(parrent, buttons, string.Format(format, args));
        }

        private MessageBoxWPF()
        {
            InitializeComponent();
        }

        public void ShowDialog(Window parrent, MessageBoxButton buttons, string caption, string text, Icon icon)
        {
            if (parrent == null || !parrent.IsLoaded)
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            else
                this.Owner = parrent;

            if (icon != null)
                ImageIcon.Source = WPFHelpers.IconToBitmapImage(icon);

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
                    BtnOk.IsCancel = true;
                    break;

                case MessageBoxButton.OKCancel:
                    BtnCancel.IsCancel = true;
                    break;

                default: throw new NotImplementedException("MessageBoxButton Yes/No are not implemented.");
            }


            this.Title = caption;
            this.TxbText.Text = text; // +"MessageBoxButton Yes/No are not implemented. as sd fsjf oweaj roiewroijMessageBoxButton Yes/No are not implemented. as sd fsjf oweaj roiewroij. MessageBoxButton Yes/No are not implemented. as sd fsjf oweaj roiewroijMessageBoxButton Yes/No are not implemented. as sd fsjf oweaj roiewroij";

            this.ShowDialog();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(DateTime.Now.ToString() + ": " + this.Title);
                sb.AppendLine("-----------------------------------------------------");
                sb.AppendLine();
                sb.AppendLine(this.TxbText.Text);
                sb.AppendLine();
                sb.AppendLine("-----------------------------------------------------");

                Clipboard.SetText(sb.ToString());
            }
        }
    }
}
