using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.ContainerStorage;
using Jpinsoft.LangTainer.ContainerStorage.Types;
using Jpinsoft.LangTainer.Types;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.Properties;
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

        #region ILTModuleView

        public string ModuleName { get { return nameof(TranslateWordModule); } }

        public void ShowModule()
        {
        }

        #endregion

        // TranslatDictItem translatDictItem = null;
        AnimSuccesFail animExtenderBtnOk;
        AnimSuccesFail animExtenderBtnFail;

        public TranslateWordModule()
        {
            InitializeComponent();

            FEContext.MainWin.Closing += MainWin_Closing;
            animExtenderBtnOk = new AnimSuccesFail(this.BtnSuccess, CN_PRE_INIT_DELAY / 4, true);
            animExtenderBtnFail = new AnimSuccesFail(this.BtnFail, CN_PRE_INIT_DELAY / 4, true);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitModule();
        }

        private void MainWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //storage?.SaveChanges();
        }

        private void InitModule()
        {
            //storage = FEContext.ModulesRepository[nameof(TranslateWordModule)];
            ScorePanel.InitScorePanel(nameof(TranslateWordModule));

            if ((targetWord = GetFromHistory()) == null)
            {
                // Last failed word
                List<WordCBO> words = FEContext.LangFromText.GetWordsBank(kp => kp.Value.PocetVyskytov > 1).OrderByDescending(kp => kp.Value.PocetVyskytov).Select(kp => kp.Value).ToList();
                int rndIndex = rnd.Next(words.Count);

                targetWord = words[rndIndex];
            }

            LblQuestion.Content = string.Format("Prelož slovo {0}", targetWord);

            //translatDictItem = MainWindow.LangFromText.GetDictItem(d => d.FirstLangWords.Contains(targetWord.Value));

            //if (translatDictItem == null)
            //    TxbAnswer.Visibility = Visibility.Hidden;
            //else
            //    TxbAddWord.Text = string.Join(Environment.NewLine, translatDictItem.SecondLangWords);

            //ScoreViewPanel.LoadData();
        }

        private WordCBO GetFromHistory()
        {
            //List<SmartData<LangModuleDataItemCBO>> history = ScorePanel.ScoreStorage.SearchSmartData(sData => sData.LastUpdate.Date < DateTime.Now.Date && sData.DataObject.FailsCount > sData.DataObject.SuccesCount);

            //if (history?.Count > 0 && rnd.Next(100) < 70)
            //{
            //    // Any random word, from max rating to min rating
            //    SmartData<LangModuleDataItemCBO> sData = history[rnd.Next(history.Count)];

            //    return FEContext.LangFromText.GetWordFromBank(sData.Key);
            //}

            return null;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (e.Target == "AZET")
                Process.Start("https://slovnik.aktuality.sk/preklad/anglicko-slovensky/?q=" + targetWord.ToString());
            else
                Process.Start("https://translate.google.com/?hl=sk&tab=wT#en/sk/" + targetWord.ToString());
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

        private void SaveScoreWord(string word, bool success)
        {
            SmartData<LangModuleDataItemCBO> todayScore = ScorePanel.GetScoreToday();

            if (success)
            {
                // Increment score today
                todayScore.DataObject.Score++;
                ScorePanel.ScoreStorage.SetSmartData(todayScore.DataObject, todayScore.Key);

                // TODO doriesit reset - co ked sa vymeni DB slov z EN na FR. Potom to nebude davat zmysel
                SmartData<LangModuleDataItemCBO> unknownWordsScoreData = ScorePanel.ScoreStorage.SearchSmartData(_ => _.DataObject.ScoreData.Contains(word)).FirstOrDefault();

                if (unknownWordsScoreData != null)
                {
                    unknownWordsScoreData.DataObject.ScoreData.Remove(word);
                    ScorePanel.ScoreStorage.SetSmartData(unknownWordsScoreData.DataObject, unknownWordsScoreData.Key);
                }
            }

            if (!success)
            {
                // PROBLEM - ScoreData nie je typu List<string> ale jsonArray
                if (!todayScore.DataObject.ScoreData.Contains(word))
                    todayScore.DataObject.ScoreData.Add(word);

                // TODo Save score
                ScorePanel.ScoreStorage.SetSmartData(todayScore.DataObject, todayScore.Key);
            }
        }

        private void SliderLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}