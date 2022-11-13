using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LangFromTextWinApp.ViewModel
{
    public class SearchResultsViewModel
    {
        public PhrasePattern PPattern { get; }

        public PhraseCBO FoundedPhrase { get; }

        public PhraseCBO WrappedSentence { get; }

        public int StartIndex { get; set; }

        public double Rating { get; set; }

        public static List<SearchResultsViewModel> FromSearchResults(List<SearchResultCBO> searchResults)
        {
            return searchResults.Select(s => new SearchResultsViewModel(s)).ToList();
        }

        public SearchResultsViewModel()
        {

        }

        public SearchResultsViewModel(SearchResultCBO sResCBO)
        {
            PPattern = sResCBO.PPattern;
            WrappedSentence = sResCBO.Sentence;
            StartIndex = sResCBO.Index;
            FoundedPhrase = sResCBO.FoundedPhrase;

            Text1 = ArrayTools.FirstUpper(string.Join(" ", WrappedSentence.Words.GetRange(0, StartIndex)));
            Text2 = string.Join(" ", FoundedPhrase.Words);

            if (string.IsNullOrEmpty(Text1))
                Text2 = ArrayTools.FirstUpper(Text2);

            Text3 = string.Join(" ", WrappedSentence.Words.GetRange(StartIndex + FoundedPhrase.Words.Count, WrappedSentence.Words.Count - StartIndex - FoundedPhrase.Words.Count)) + ".";

            Rating = sResCBO.Rating;
        }

        public string Text1Short
        {
            get
            {
                return (Text1?.Length > 20) ? "..." + Text1.Substring(Text1.Length - 20, 20) : Text1;
            }
        }

        public string Text3Short
        {
            get
            {
                return (Text3?.Length > 20) ? Text3.Substring(0, 20) + "..." : Text3;
            }
        }

        public string Text1 { get; set; }

        public string Text2 { get; set; }

        public string Text3 { get; set; }

        public Visibility SeparatorVisibility { get; set; } = Visibility.Collapsed;
    }
}
