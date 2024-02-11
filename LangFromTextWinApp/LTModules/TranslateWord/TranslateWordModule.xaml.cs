﻿using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.ContainerStorage;
using Jpinsoft.LangTainer.ContainerStorage.Types;
using Jpinsoft.LangTainer.Types;
using Jpinsoft.LangTainer.Utils;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LangFromTextWinApp.LTModules.TranslateWord
{
    /// <summary>
    /// Interaction logic for TranslateWordModule.xaml
    /// </summary>
    public partial class TranslateWordModule : UserControl, ILTModuleView
    {
        static Random rnd = new Random();
        WordCBO targetWord = null;
        private const int CN_PRE_INIT_DELAY = 1000;
        private const int FROM_HISTORY_PERCENT_PROBABILITY = 50;
        AnimSuccesFail animExtender;
        // TranslatDictItem translatDictItem = null;
        AnimSuccesFail animExtenderBtnOk;
        AnimSuccesFail animExtenderBtnFail;

        #region ILTModuleView

        public string ModuleName { get { return nameof(TranslateWordModule); } }

        public void ShowModule()
        {
        }

        #endregion

        public TranslateWordModule()
        {
            InitializeComponent();

            FEContext.MainWin.Closing += MainWin_Closing;
            animExtenderBtnOk = new AnimSuccesFail(this.BtnSuccess, CN_PRE_INIT_DELAY / 4, true);
            animExtenderBtnFail = new AnimSuccesFail(this.BtnFail, CN_PRE_INIT_DELAY / 4, true);

            animExtender = new AnimSuccesFail(this.LabelTargetWord, CN_PRE_INIT_DELAY, false);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitModule();
        }

        private void MainWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private async void BtnSuccess_Click(object sender, RoutedEventArgs e)
        {
            SaveScoreWord(targetWord.Value.ToString(), true);

            animExtenderBtnOk.AnimSuccess();
            await Task.Delay((int)(CN_PRE_INIT_DELAY * 1.5f));

            InitModule();
        }

        private async void BtnFail_Click(object sender, RoutedEventArgs e)
        {
            SaveScoreWord(targetWord.Value.ToString(), false);

            animExtenderBtnFail.AnimFail();
            await Task.Delay((int)(CN_PRE_INIT_DELAY * 1.5f));

            InitModule();
        }

        private void SliderLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.IsLoaded)
                InitModule();
        }

        private void TxbTargetWord_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WPFHelpers.OpenTranslator(targetWord.ToString());
        }

        private void InitModule()
        {
            ScorePanel.InitScorePanel(nameof(TranslateWordModule));
            int minRating = (4 - (int)SliderLevel.Value) * 500; // Rating from 1500 to 500
            LabelTargetWord.Visibility = BtnSuccess.Visibility = BtnFail.Visibility = Visibility.Visible;

            // Last failed word
            List<WordCBO> words = FEContext.LangFromText.GetWordsBank(kp => kp.Value.Rating > minRating).Select(kp => kp.Value).ToList();

            if (words.Count < 10)
            {
                LblQuestion.Text = Properties.Resources.T204;
                LabelTargetWord.Visibility = BtnSuccess.Visibility = BtnFail.Visibility = Visibility.Hidden;

                return;
            }

            if ((targetWord = GetFromHistory()) == null)
                targetWord = words[rnd.Next(words.Count)];

            LblQuestion.Text = Properties.Resources.T205;
            TxbTargetWord.Text = targetWord.ToString();
            animExtender.AnimSuccess();
        }

        private WordCBO GetFromHistory()
        {
            List<string> unknownWords = new List<string>();
            ScorePanel.ScoreStorage.SearchSmartData(_ => _.Created.Date < DateTime.Now.Date && _.DataObject.ScoreData.Count > 0)
                .ForEach(_ => unknownWords.AddRange(_.DataObject.ScoreData));

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

        private void SaveScoreWord(string word, bool success)
        {
            SmartData<LangModuleDataItemCBO> todayScore = ScorePanel.GetScoreToday();

            if (success)
            {
                // Increment score today
                todayScore.DataObject.Score++;
                ScorePanel.ScoreStorage.SetSmartData(todayScore.DataObject, todayScore.Key);

                RemoveWordFormScoreData(word);
            }

            if (!success)
            {
                if (!todayScore.DataObject.ScoreData.Contains(word))
                {
                    todayScore.DataObject.ScoreData.Add(word);
                    ScorePanel.ScoreStorage.SetSmartData(todayScore.DataObject, todayScore.Key);
                }
            }
        }

        private void RemoveWordFormScoreData(string word)
        {
            // TODO doriesit reset - co ked sa vymeni DB slov z EN na FR. Potom to nebude davat zmysel
            SmartData<LangModuleDataItemCBO> wordsScoreData = ScorePanel.ScoreStorage.SearchSmartData(_ => _.DataObject.ScoreData.Contains(word)).FirstOrDefault();

            if (wordsScoreData != null)
            {
                wordsScoreData.DataObject.ScoreData.Remove(word);
                ScorePanel.ScoreStorage.SetSmartData(wordsScoreData.DataObject, wordsScoreData.Key);
            }
        }
    }
}