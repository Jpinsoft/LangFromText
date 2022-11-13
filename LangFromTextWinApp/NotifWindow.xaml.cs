using LangFromTextWinApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Threading;

namespace LangFromTextWinApp
{
    /// <summary>
    /// Interaction logic for NotifWindow.xaml
    /// </summary>
    public partial class NotifWindow : Window
    {
        public static bool IsOpened { get; set; } = false;

        public NotifWindow()
        {
            InitializeComponent();
        }

        private void BtnShowLT_Click(object sender, RoutedEventArgs e)
        {
            FEContext.MainWin.ShowOnTop();
            FEContext.MNavigator.ShowRandomLangModule();

            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IsOpened = true;
            this.Left = SystemParameters.VirtualScreenWidth - this.Width - 10;
            this.Top = SystemParameters.VirtualScreenHeight - this.Height - 40;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsOpened = false;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
