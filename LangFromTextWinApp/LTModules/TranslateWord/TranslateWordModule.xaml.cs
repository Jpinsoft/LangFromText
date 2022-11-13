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
        AnimSuccesFail animExtender;
        ISmartStorage<LangModuleDataItemCBO> storage;

        public TranslateWordModule()
        {
            InitializeComponent();

            FEContext.MainWin.Closing += MainWin_Closing;
            animExtender = new AnimSuccesFail(this.MainPanel, CN_PRE_INIT_DELAY, true);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitModule();
        }

        private void MainWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            storage?.SaveChanges();
        }

        private void InitModule()
        {
            storage = FEContext.ModulesRepository[nameof(TranslateWordModule)];

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
            List<SmartData<LangModuleDataItemCBO>> history = storage.SearchSmartData(sData => sData.LastUpdate.Date < DateTime.Now.Date && sData.DataObject.FailsCount > sData.DataObject.SuccesCount);

            if (history?.Count > 0 && rnd.Next(100) < 70)
            {
                // Any random word, from max rating to min rating
                SmartData<LangModuleDataItemCBO> sData = history[rnd.Next(history.Count)];

                return FEContext.LangFromText.GetWordFromBank(sData.Key);
            }

            return null;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (e.Target == "AZET")
                Process.Start("https://slovnik.aktuality.sk/preklad/anglicko-slovensky/?q=" + targetWord.ToString());
            else
                Process.Start("https://translate.google.com/?hl=sk&tab=wT#en/sk/" + targetWord.ToString());
        }

        private async void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            SaveResult(true);

            animExtender.AnimSuccess();
            await Task.Delay((int)(CN_PRE_INIT_DELAY * 1.5f));

            InitModule();
        }

        private async void BtnProblem_Click(object sender, RoutedEventArgs e)
        {
            SaveResult(false);

            animExtender.AnimFail();
            await Task.Delay((int)(CN_PRE_INIT_DELAY * 1.5f));

            InitModule();
        }

        private void SaveResult(bool success)
        {
            SmartData<LangModuleDataItemCBO> sData = storage.GetSmartData(targetWord.Value.ToString());
            LangModuleDataItemCBO tWordResult = sData != null ? sData.DataObject : new LangModuleDataItemCBO { };

            if (success)
                tWordResult.SuccesCount++;
            else
                tWordResult.FailsCount++;

            // TODo Save score
            storage.SetSmartData(tWordResult, targetWord.Value.ToString());
        }

    }
}