using FontAwesome.WPF;
using Jpinsoft.LangTainer;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LangFromTextWinApp.ViewModel
{
    public class LangStartPageViewModel : ViewModelBase
    {
        public LangStartPageViewModel()
        {
            DataConnString = Settings.Default.DataConnString;
            WordsCount = FEContext.LangFromText.WordsCount;
            SentencesCount = FEContext.LangFromText.GetPhrases().Count;
            TextSourcesCount = FEContext.LangFromText.GetTextSources().Count;
            IsDataModelEmpty = false;

            TitleText = Resources.T018;
            WordsCountText = string.Format(Resources.T010, WordsCount);
            SourcesIndexText = string.Format(Resources.T012, TextSourcesCount, FEContext.LangFromText.TextSourcesPer1000WordsIndex.ToString("F1"));
            TrainingAgentIcon = Settings.Default.ActivateLTModuleIntervalMinutes > 0 ? FontAwesomeIcon.PlayCircle : FontAwesomeIcon.StopCircle;
            TrainingAgentText = Settings.Default.ActivateLTModuleIntervalMinutes > 0 ? Resources.T054 : Resources.T055;

            if (!FEContext.LangFromText.HasEnoughtWords) // Min 500 words
            {
                // WordsCountText = string.Format(Resources.T011, WordsCount);
                // WordsCountIcon = FontAwesomeIcon.Warning;
                IsDataModelEmpty = true;
                IsDataModelEmptyText = string.Format(Resources.T053, WordsCount, LangFromTextManager.CN_MIN_WORDS_COUNT);
                TitleText = Resources.T009;
                //MainIconColor = Brushes.DarkRed;
            }

            if (FEContext.LangFromText.TextSourcesPer1000WordsIndex <= LangFromTextManager.CN_MIN_TEXTSOURCES_INDEX)
            {
                // MainIconColor = Brushes.Orange;

                SourcesIndexIcon = FontAwesomeIcon.Warning;
                SourcesIndexText = string.Format(Resources.T013, FEContext.LangFromText.TextSourcesPer1000WordsIndex.ToString("F1"));

                TitleText = Resources.T009;
            }

            IsDataModelFilled = !IsDataModelEmpty;
        }

        public Brush MainIconColor { get; set; } = Brushes.DarkBlue;

        public string TitleText { get; set; }

        public FontAwesomeIcon WordsCountIcon { get; set; } = FontAwesomeIcon.CheckCircle;

        public string WordsCountText { get; set; }

        public FontAwesomeIcon SourcesIndexIcon { get; set; } = FontAwesomeIcon.CheckCircle;

        public string SourcesIndexText { get; set; }

        public FontAwesomeIcon TrainingAgentIcon { get; set; } = FontAwesomeIcon.CheckCircle;

        public string TrainingAgentText { get; set; }

        public string DataConnString { get; set; }

        public int WordsCount { get; set; }

        public int SentencesCount { get; set; }

        public int TextSourcesCount { get; set; }

        public int LangModulesTotalScore { get; set; }

        public bool IsDataModelEmpty { get; set; }

        public string IsDataModelEmptyText { get; set; }

        public bool IsDataModelFilled { get; set; }
    }
}
