using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.LangAdapters;
using Jpinsoft.LangTainer.Types;
using LangFromTextWinApp.Controls;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.View.Popup;
using LangFromTextWinApp.ViewModel;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace LangFromTextWinApp.View
{
    /// <summary>
    /// Interaction logic for LangDataView.xaml
    /// </summary>
    public partial class LangDataView : UserControl
    {
        public LangDataViewModel ViewModel { get { return this.DataContext as LangDataViewModel; } }
        private OpenFileDialog openFileDialog = new OpenFileDialog();

        ILangAdapter langAdapter = new FileLangAdapter();

        public LangDataView()
        {
            InitializeComponent();

            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // await Task.Run(() => this.DataContext = new LangDataViewModel());

            this.DataContext = new LangDataViewModel();
        }

        private async void BtnIndexFile_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() == true)
            {
                await ViewModel.IndexTextSource(new ProgressContentOverlayControl(this), langAdapter.GetTextSources(openFileDialog.FileName).First());
            }
        }

        private void BtnIndexWeb_Click(object sender, RoutedEventArgs e)
        {
            new IndexWebWindow(FEContext.MainWin).ShowDialog();
            ViewModel.Init();
        }

        private void TxbSearchWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(TxbSearchWord.Text))
            {
                ViewModel.SearchWord(TxbSearchWord.Text);
                return;
            }
        }

        private void TxbSearchWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxbSearchWord.Text))
                ViewModel.SearchWord(TxbSearchWord.Text);
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SearchWord(TxbSearchWord.Text);
        }

        private async void BtnRemoveDatasource_Click(object sender, RoutedEventArgs e)
        {
            TextSourceCBO tSource = ((Button)sender).Tag as TextSourceCBO;

            if (tSource != null && MessageBoxWPF.ShowWarningFormat(FEContext.MainWin, MessageBoxButton.OKCancel, Properties.Resources.T019, tSource.Address) == true)
            {
                await ViewModel.RemoveTextSource(new ProgressContentOverlayControl(this), new List<TextSourceCBO> { tSource });
            }
        }

        private async void lvSources_KeyDown(object sender, KeyEventArgs e)
        {
            TextSourceCBO tSource = lvSources.SelectedItem as TextSourceCBO;

            if (tSource != null && e.Key == Key.Delete && MessageBoxWPF.ShowWarningFormat(FEContext.MainWin, MessageBoxButton.OKCancel, Properties.Resources.T066, lvSources.SelectedItems.Count) == true)
            {
                await ViewModel.RemoveTextSource(new ProgressContentOverlayControl(this), lvSources.SelectedItems.Cast<TextSourceCBO>().ToList());
            }

            if (tSource != null && e.Key == Key.Enter)
            {
                await OpenTextSourcePopup(ViewModel.SelectedTextSource);
            }
        }

        private async void lvSources_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await OpenTextSourcePopup(ViewModel.SelectedTextSource);
        }

        private async Task OpenTextSourcePopup(TextSourceCBO textSource)
        {
            if (textSource != null)
            {
                PopUpTextSourceDetail.StaysOpen = true;
                PopUpTextSourceDetail.IsOpen = true;
                UserControlTextSourceDetail.Init(textSource, () => PopUpTextSourceDetail.IsOpen = false);

                await Task.Delay(500);
                PopUpTextSourceDetail.StaysOpen = false;
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Zial nefunguje nastavenie FOCUS na popup
            if (e.Key == Key.Escape)
                PopUpTextSourceDetail.IsOpen = false;
        }

        private void lvSources_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try { TextSourceColumn.Width = e.NewSize.Width - 120; }
            catch { }
        }

        ListSortDirection _lastDirection = ListSortDirection.Descending;

        private void lvSources_HeaderClick(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                direction = (_lastDirection == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;

                ICollectionView dataView = CollectionViewSource.GetDefaultView(lvSources.ItemsSource);

                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(nameof(TextSourceCBO.Created), direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();

                _lastDirection = direction;
            }
        }
    }

}
