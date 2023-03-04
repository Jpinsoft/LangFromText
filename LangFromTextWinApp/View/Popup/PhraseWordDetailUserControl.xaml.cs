using Jpinsoft.LangTainer.CBO;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.ViewModel;
using LangFromTextWinApp.ViewModel.Popup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace LangFromTextWinApp.View.Popup
{
    /// <summary>
    /// Interaction logic for PhraseWordDetailUserControl.xaml
    /// </summary>
    public partial class PhraseWordDetailUserControl : UserControl
    {
        public PhraseWordDetailViewModel ViewModel { get { return this.DataContext as PhraseWordDetailViewModel; } }

        public PhraseWordDetailUserControl()
        {
            InitializeComponent();
        }

        public void Init(Tuple<PhraseCBO, PhraseCBO> sentencePhrase, Action onHide)
        {
            List<TextSourceCBO> tSources = FEContext.LangFromText.GetTextSources(tSource => tSource.Sentences.Contains(sentencePhrase.Item1));
            SearchResultsViewModel searchResultsViewModel = new SearchResultsViewModel(new SearchResultCBO { FoundedPhrase = sentencePhrase.Item2, Sentence = sentencePhrase.Item1 });

            this.DataContext = new PhraseWordDetailViewModel { SearchResults = searchResultsViewModel, TextSources = tSources, OnHideAction = onHide };
        }

        public void Init(SearchResultsViewModel searchResultsViewModel, Action onHide)
        {
            List<TextSourceCBO> tSources = FEContext.LangFromText.GetTextSources(tSource => tSource.Sentences.Contains(searchResultsViewModel.WrappedSentence));

            this.DataContext = new PhraseWordDetailViewModel { SearchResults = searchResultsViewModel, TextSources = tSources, OnHideAction = onHide };
        }

        private void BtnHide_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Hide();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextSourceCBO tSource = (sender as TextBlock)?.Tag as TextSourceCBO;

            if (tSource != null)
            {
                if (tSource.TextSourceType == Jpinsoft.LangTainer.Enums.TextSourceTypeEnum.File && !File.Exists(tSource.Address))
                {
                    MessageBoxWPF.ShowInfo(FEContext.MainWin, MessageBoxButton.OK, Properties.Resources.T020);
                    return;
                }

                Process.Start(tSource.Address);
            }
        }

        private void Run_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WPFHelpers.OpenTranslator(((Run)sender).Text);
        }
    }
}
