using Jpinsoft.LangTainer;
using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.Types;
using LangFromTextWinApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LangFromTextWinApp.ViewModel
{
    public class LangPhraseCheckViewModel : ViewModelBase
    {
        #region Fields&Props

        private double rating = 100;

        public double Rating
        {
            get { return rating; }
            set { SetProperty(ref rating, value); }
        }

        private double ratingPercent = 0;

        public double RatingPercent
        {
            get { return ratingPercent; }
            set { SetProperty(ref ratingPercent, value); }
        }

        private List<SearchResultsViewModel> searchResults = new List<SearchResultsViewModel>();

        public List<SearchResultsViewModel> SearchResults
        {
            get { return searchResults; }
            set { SetProperty(ref searchResults, value); }
        }


        private int resultsCount;

        public int ResultsCount
        {
            get { return resultsCount; }
            set { SetProperty(ref resultsCount, value); }
        }

        private bool canCheckPhrase = false;

        public bool CanCheckPhrase
        {
            get { return canCheckPhrase; }
            set { SetProperty(ref canCheckPhrase, value); }
        }

        private string phraseText = string.Empty;

        public string PhraseText
        {
            get { return phraseText; }
            set
            {
                CheckInputStringPhrase(value);
                SetProperty(ref phraseText, value);
            }
        }

        #endregion

        public LangPhraseCheckViewModel()
        {
            CopyClipboardCommand = new Action(CopyToClipboard);
        }

        public Action CopyClipboardCommand { get; private set; }

        private void CheckInputStringPhrase(string inputPhrase)
        {
            if (string.IsNullOrWhiteSpace(inputPhrase))
            {
                CanCheckPhrase = false;
                return;
            }

            List<string> parsedSen = FEContext.LangFromText.SentenceParser.ParseToWords(inputPhrase);

            if (parsedSen?.Count == 0 || parsedSen.Count > 12)
            {
                CanCheckPhrase = false;
                return;
            }

            CanCheckPhrase = true;
        }

        public async Task CheckPhrase(IProgressControl progressControl)
        {
            if (!CanCheckPhrase)
                return;

            try
            {
                progressControl.ShowProgress(false);

                await Task.Run(() =>
                {
                    // TODO - ParseSentence vyzaduje minimalne 2 slovnu frazu !!!
                    // List<string> parsedSen = FEContext.LangFromText.SentenceParser.ParseToWords(PhraseText);
                    PhraseCBO inputPhrase = FEContext.LangFromText.ParseTextToPhrase(PhraseText);
                    int maxAsterixCount = inputPhrase.Words.Count / 2;

                    List<PhrasePattern> ppList = FEContext.LangFromText.GenerateAsterixPatterns(PhraseText, maxAsterixCount);
                    List<SearchResultsViewModel> resView = new List<SearchResultsViewModel>();

                    PhrasePattern prevPP = null;

                    foreach (PhrasePattern pPattern in ppList)
                    {
                        List<SearchResultCBO> sResCBO = FEContext.LangFromText.ContainsPhrase(pPattern);
                        List<SearchResultsViewModel> searchResults = SearchResultsViewModel.FromSearchResults(sResCBO);

                        if (prevPP?.AsterixCount != pPattern.AsterixCount && pPattern?.AsterixCount > 0)
                        {
                            // resView.Add(new SearchResultsViewModel { SeparatorVisibility = Visibility.Visible, Text1 = "----------------------", Text2 = $"{pPattern.AsterixCount}*", Text3 = "-----------------------------------" });
                            resView.Add(new SearchResultsViewModel { SeparatorVisibility = Visibility.Visible, Text1 = "***** Similar", Text3 = "matches *****" });
                        }

                        resView.AddRange(searchResults);
                        prevPP = pPattern;
                    }

                    // Contains nezavisle od poradia
                    List<SearchResultCBO> sRes2CBO = FEContext.LangFromText.ContainsWords(inputPhrase.Words.ToArray());
                    if (sRes2CBO.Count > 0)
                    {
                        resView.Add(new SearchResultsViewModel { SeparatorVisibility = Visibility.Visible, Text1 = "********** Contains", Text3 = "words **********" });
                        resView.AddRange(SearchResultsViewModel.FromSearchResults(sRes2CBO));
                    }

                    resView = UniqueResults(resView);

                    Rating = resView.Sum(r => r.Rating);
                    RatingPercent = LangFromTextManager.RatingSumToPercentage(Rating);
                    Rating = LangFromTextManager.RatingSumRatingTotal(Rating);

                    SearchResults = resView;
                    ResultsCount = SearchResults.Count(sr => sr.FoundedPhrase != null);
                });

            }
            finally
            {
                progressControl.CloseProgress();
            }
        }

        private static List<SearchResultsViewModel> UniqueResults(List<SearchResultsViewModel> resView)
        {
            List<SearchResultsViewModel> uniqueResView = new List<SearchResultsViewModel>();

            foreach (SearchResultsViewModel item in resView)
            {
                if (item.FoundedPhrase == null || uniqueResView.FirstOrDefault(_ => _.FoundedPhrase == item.FoundedPhrase &&
                _.WrappedSentence == item.WrappedSentence &&
                _.StartIndex == item.StartIndex) == null)
                {
                    uniqueResView.Add(item);
                }
            }

            resView = uniqueResView;
            return resView;
        }

        public void CopyToClipboard()
        {
            StringBuilder sb = new StringBuilder();

            searchResults.ForEach(item => sb.AppendFormat("{0} {1} {2}\t{3}\n", item.Text1Short, item.Text2, item.Text3Short, item.PPattern != null ? item.PPattern.ToString() : "N/A"));

            Clipboard.SetText(sb.ToString());
        }
    }
}
