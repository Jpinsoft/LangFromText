using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.Types;
using LangFromTextWinApp.Controls;
using LangFromTextWinApp.View.Popup;
using LangFromTextWinApp.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LangFromTextWinApp.View
{
    /// <summary>
    /// Interaction logic for LangPhraseCheckView.xaml
    /// </summary>
    public partial class LangPhraseCheckView : UserControl
    {
        public LangPhraseCheckViewModel ViewModel { get { return this.DataContext as LangPhraseCheckViewModel; } }

        public LangPhraseCheckView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new LangPhraseCheckViewModel();
        }

        private async void TxbCheckPhrase_KeyDown(object sender, KeyEventArgs e)
        {
            //ViewModel.Rating = 0;
            //for (int i = 0; i < 100; i++)
            //{
            //    ViewModel.Rating++;
            //    await Task.Delay(100);
            //}

            if (e.Key == Key.Enter)
            {
                await ViewModel.CheckPhrase(new ProgressContentOverlayControl(this));
            }
        }

        private async void BtnCheckPhrase_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.CheckPhrase(new ProgressContentOverlayControl(this));
        }

        private void lvPhrases_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gridView = listView.View as GridView;
            double actualWidth = listView.ActualWidth - gridView.Columns[1].ActualWidth - 80;

            if (actualWidth > 40)
                gridView.Columns[0].Width = actualWidth;
        }

        private async void Run_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Run)?.Tag as SearchResultsViewModel != null)
            {
                PopUpPhraseDetail.StaysOpen = true;
                PopUpPhraseDetail.IsOpen = true;
                UserControlPhraseDetail.Init(((sender as Run).Tag as SearchResultsViewModel), () => PopUpPhraseDetail.IsOpen = false);

                await Task.Delay(500);
                PopUpPhraseDetail.StaysOpen = false;
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Zial nefunguje nastavenie FOCUS na popup
            if (e.Key == Key.Escape)
                PopUpPhraseDetail.IsOpen = false;
        }

        private void BtnCopyClipboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyToClipboard();
        }

        private void CopyCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.CopyToClipboard();
        }
    }
}
