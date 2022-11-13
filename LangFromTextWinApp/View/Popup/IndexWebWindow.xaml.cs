using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.LangAdapters;
using Jpinsoft.LangTainer.LangAdapters.Html;
using Jpinsoft.LangTainer.Types;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.Properties;
using LangFromTextWinApp.ViewModel.Popup;
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

namespace LangFromTextWinApp.View.Popup
{
    /// <summary>
    /// Interaction logic for IndexWebWindow.xaml
    /// </summary>
    public partial class IndexWebWindow : Window
    {
        public IndexWebViewModel ViewModel { get { return this.DataContext as IndexWebViewModel; } }

        public IndexWebWindow(Window owner)
        {
            this.Owner = owner;
            InitializeComponent();

            this.DataContext = new IndexWebViewModel();
        }

        public IndexWebWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadHistory();
        }

        private async void BtnIndexWeb_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.IndexWeb(CbTargetURL.Text, (int)SliderLevel.Value, (int)SliderMaxLimit.Value, TxbOnlyURLContains.Text, TxbOnlyURLNotContains.Text);
        }

        private void BtnRemoveFromHistory_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveFromScanHistory((sender as Button).Tag.ToString());
        }

        private void LbScanOutput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.IsKeyDown(Key.LeftCtrl) && LbScanOutput.SelectedItems.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                foreach (object o in LbScanOutput.SelectedItems)
                    sb.AppendLine(o.ToString());

                Clipboard.SetText(sb.ToString());
            }
        }
    }
}
