using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.Utils;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LangFromTextWinApp.LTModules.Vocabulary
{
    /// <summary>
    /// Interaction logic for VocabularyModule.xaml
    /// </summary>
    public partial class VocabularyModule : UserControl, ILTModuleView
    {
        static Random rnd = new Random();
        WordCBO targetWord = null;
        private const int CN_PRE_INIT_DELAY = 1000;
        private const int FROM_HISTORY_PERCENT_PROBABILITY = 50;
        AnimSuccesFail animExtender;
        // TranslatDictItem translatDictItem = null;
        AnimSuccesFail animExtenderBtnOk;
        AnimSuccesFail animExtenderBtnFail;
        private List<WordCBO> wordsToday = new List<WordCBO>();

        #region ILTModuleView

        public string ModuleName { get { return nameof(VocabularyModule); } }

        public void ShowModule()
        {
        }

        #endregion

        public VocabularyModule()
        {
            InitializeComponent();

            animExtenderBtnOk = new AnimSuccesFail(this.BtnSuccess, CN_PRE_INIT_DELAY / 6, true);
            animExtenderBtnFail = new AnimSuccesFail(this.BtnFail, CN_PRE_INIT_DELAY / 6, true);
            animExtender = new AnimSuccesFail(this.LabelTargetWord, CN_PRE_INIT_DELAY / 4, false);

            SliderLevel.Value = Properties.Settings.Default.TranslateWordModuleLevel;
        }

        #region Events

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitModule();
        }

        private async void BtnSuccess_Click(object sender, RoutedEventArgs e)
        {
            UpdateScore(targetWord.Value.ToString(), true);

            animExtenderBtnOk.AnimSuccess();
            await Task.Delay((int)(CN_PRE_INIT_DELAY / 4f));

            InitModule();
        }

        private async void BtnFail_Click(object sender, RoutedEventArgs e)
        {
            UpdateScore(targetWord.Value.ToString(), false);

            animExtenderBtnFail.AnimFail();
            await Task.Delay((int)(CN_PRE_INIT_DELAY / 4f));

            InitModule();
        }

        private void SliderLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.IsLoaded)
            {
                InitModule();
                Settings.Default.TranslateWordModuleLevel = (int)SliderLevel.Value;
            }
        }

        private void TxbTargetWord_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WPFHelpers.OpenTranslator(targetWord.ToString());
        }

        #endregion

        private void InitModule()
        {
            ScorePanel.InitScorePanel(nameof(VocabularyModule), Properties.Resources.T203, (int)SliderLevel.Value);
            double minRating = Math.Pow((4 - (int)SliderLevel.Value), 1.5f) * 200; // Min rating from 1039 to 200
            LabelTargetWord.Visibility = BtnSuccess.Visibility = BtnFail.Visibility = Visibility.Visible;

            List<WordCBO> words = FEContext.LangFromText.GetWordsBank(kp => kp.Value.Rating > minRating).Select(kp => kp.Value).ToList();

            if (words.Count < 10)
            {
                LblQuestion.Text = Properties.Resources.T204;
                LabelTargetWord.Visibility = BtnSuccess.Visibility = BtnFail.Visibility = Visibility.Hidden;

                return;
            }

            // Last failed word from history
            if ((targetWord = GetFromHistory()) == null)
            {
                for (int i = 0; i < 10; i++)
                {
                    targetWord = words[rnd.Next(words.Count)];

                    // Prevent repetition of words
                    if (!wordsToday.Contains(targetWord))
                        break;
                }
            }

            wordsToday.Add(targetWord);

            LblQuestion.Text = Properties.Resources.T205;
            TxbTargetWord.Text = targetWord.ToString();
            animExtender.AnimSuccess();
        }

        private WordCBO GetFromHistory()
        {
            List<string> unknownWords = new List<string>();
            ScorePanel.LevelData.Where(_ => _.Created.Date < DateTime.Now.Date && _.ScoreData.Count > 0).ToList()
                .ForEach(_ => unknownWords.AddRange(_.ScoreData));

            if (rnd.Next(100) < FROM_HISTORY_PERCENT_PROBABILITY && unknownWords.Count > 0)
            {
                // Any random word, from max rating to min rating
                unknownWords.Shuffle(rnd);

                foreach (string w in unknownWords)
                {
                    WordCBO wordCBO = FEContext.LangFromText.GetWordFromBank(w, false);

                    if (wordCBO != null)
                        return wordCBO;
                    else
                        RemoveWordFormScoreData(w);
                }
            }

            return null;
        }

        private void UpdateScore(string word, bool success)
        {
            LangModuleDataItemCBO scoreToday = ScorePanel.GetScoreToday();

            if (success)
            {
                // Increment score today
                scoreToday.Score++;
                ScorePanel.ScoreStorage[scoreToday.Key] = scoreToday;

                RemoveWordFormScoreData(word);
            }

            if (!success)
            {
                if (!scoreToday.ScoreData.Contains(word))
                {
                    scoreToday.ScoreData.Add(word);
                    ScorePanel.ScoreStorage[scoreToday.Key] = scoreToday;
                }
            }
        }

        private void RemoveWordFormScoreData(string word)
        {
            // TODO doriesit reset - co ked sa vymeni DB slov z EN na FR. Potom to nebude davat zmysel
            LangModuleDataItemCBO wordsScoreData = ScorePanel.LevelData.FirstOrDefault(_ => _.ScoreData.Contains(word));

            if (wordsScoreData != null)
            {
                wordsScoreData.ScoreData.Remove(word);
                ScorePanel.ScoreStorage[wordsScoreData.Key] = wordsScoreData;
            }
        }
    }
}