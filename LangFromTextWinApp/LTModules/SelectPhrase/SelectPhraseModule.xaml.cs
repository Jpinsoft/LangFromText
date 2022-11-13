using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.ContainerStorage.Types;
using Jpinsoft.LangTainer.Types;
using Jpinsoft.LangTainer.Utils;
using LangFromTextWinApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LangFromTextWinApp.LTModules.SelectPhrase
{
    /// <summary>
    /// Interaction logic for SelectPhraseModule.xaml
    /// </summary>
    public partial class SelectPhraseModule : UserControl, ILTModuleView
    {
        private Random rnd = new Random();
        private const int CN_PRE_INIT_DELAY = 700;
        private const int DLZKA_ZOBRAZENEJ_FRAZY = 6;

        bool answShowed = false;
        ISmartStorage<LangModuleDataItemCBO> storage;

        public SelectPhraseModule()
        {
            InitializeComponent();
        }

        #region ILTModuleView

        public string ModuleName { get { return nameof(SelectPhraseModule); } }

        public void ShowModule()
        {
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            wPanelMain.Children.Clear();
            InitModule();
        }

        private void SliderLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CheckResult();
                e.Handled = true;
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            CheckResult();
        }

        private async void InitModule()
        {
            storage = FEContext.ModulesRepository[nameof(SelectPhraseModule)];
            ScorePanel.InitScorePanel(nameof(SelectPhraseModule));
            // Generate GUI
            wPanelMain.Children.Clear();
            answShowed = false;

            List<PhraseCBO> rndSentences = FEContext.LangFromText.GetRandomSentences(4, 10);

            IList<int> rndCharsIndex = RandomTools.Shuffle<int>(RandomTools.GeneratePostupnost<int>(0, rndSentences.Count - 1), rnd);
            rndCharsIndex = rndCharsIndex.Take(1).ToList();

            DoubleAnimation moveDoubleAnimation = Application.Current.FindResource(FEConstants.RESKEY_SelectPhraseModuleAnimation) as DoubleAnimation;
            SolidColorBrush pallete2 = Application.Current.FindResource(FEConstants.RESKEY_Pallete2) as SolidColorBrush;
            SolidColorBrush pallete3 = Application.Current.FindResource(FEConstants.RESKEY_Pallete3) as SolidColorBrush;

            for (int i = 0; i < rndSentences.Count; i++)
            {
                bool makeIncorrectPhrase = !rndCharsIndex.Contains(i);

                ToggleButton tButton = GeneratePhraseButton(rndSentences[i], makeIncorrectPhrase);
                tButton.Background = pallete2;

                wPanelMain.Children.Add(tButton);

                if ((i % 2) != 0)
                {
                    tButton.AnimTranslateX(-moveDoubleAnimation.From.Value * (rnd.Next(80, 100) / 100f), -moveDoubleAnimation.To.Value * (rnd.Next(50, 100) / 100f), moveDoubleAnimation.EasingFunction, moveDoubleAnimation.Duration.TimeSpan.TotalMilliseconds);
                    tButton.Background = pallete3;
                }
                else
                    tButton.AnimTranslateX(moveDoubleAnimation.From.Value * (rnd.Next(80, 100) / 100f), moveDoubleAnimation.To.Value * (rnd.Next(50, 100) / 100f), moveDoubleAnimation.EasingFunction, moveDoubleAnimation.Duration.TimeSpan.TotalMilliseconds);
            }

            await Task.Delay((int)moveDoubleAnimation.Duration.TimeSpan.TotalMilliseconds);
            wPanelMain.Children[0].Focus();
        }

        private ToggleButton GeneratePhraseButton(PhraseCBO origPhrase, bool makeIncorrectPhrase)
        {
            PhraseCBO origSubPhrase;
            bool isFromStart = (rnd.Next(100) > 50);

            if (isFromStart)  // END WITH
                origSubPhrase = new PhraseCBO { Words = origPhrase.Words.Take(DLZKA_ZOBRAZENEJ_FRAZY).ToList() };
            else // START WITH
                origSubPhrase = new PhraseCBO { Words = origPhrase.Words.GetRange(origPhrase.Words.Count - DLZKA_ZOBRAZENEJ_FRAZY, DLZKA_ZOBRAZENEJ_FRAZY).ToList() };

            PhraseCBO phrase = origSubPhrase;

            if (makeIncorrectPhrase)
            {
                do // Pokazenie frazy
                {
                    phrase = new PhraseCBO { Id = -1, Words = new List<WordCBO>(origSubPhrase.Words) };
                    phrase.Words.Shuffle(rnd);
                    phrase.Words.Shuffle(rnd);

                } while (phrase == origSubPhrase);
            }

            StringBuilder sbBtnText = null;

            if (isFromStart)
            {
                sbBtnText = new StringBuilder(phrase.TextResult + " ...");
                sbBtnText[0] = char.ToUpper(sbBtnText[0]);
            }
            else
            {
                sbBtnText = new StringBuilder("... " + phrase.TextResult.Trim() + ".");
            }

            ToggleButton tButton = new ToggleButton { Tag = !makeIncorrectPhrase, Style = Application.Current.FindResource(FEConstants.RESKEY_PhraseToggleButtonStyle) as Style };

            TextBlock txBlock = new TextBlock();
            Run runContentText = new Run { Text = sbBtnText.ToString(), Tag = new Tuple<PhraseCBO, PhraseCBO>(origPhrase, origSubPhrase) };
            txBlock.Inlines.Add(runContentText);

            tButton.Content = txBlock;

            return tButton;
        }

        private async void RunContent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Run)?.Tag as Tuple<PhraseCBO, PhraseCBO> != null)
            {
                PopUpPhraseDetail.StaysOpen = true;
                PopUpPhraseDetail.IsOpen = true;
                UserControlPhraseDetail.Init(((sender as Run).Tag as Tuple<PhraseCBO, PhraseCBO>), () => PopUpPhraseDetail.IsOpen = false);

                await Task.Delay(500);
                PopUpPhraseDetail.StaysOpen = false;
            }
        }

        // Phrase Detail Popup
        //private async void BtnShowPhrase_Click(object sender, RoutedEventArgs e)
        //{
        //    if ((sender as Button)?.Tag as Tuple<PhraseCBO, PhraseCBO> != null)
        //    {
        //        PopUpPhraseDetail.StaysOpen = true;
        //        PopUpPhraseDetail.IsOpen = true;
        //        UserControlPhraseDetail.Init(((sender as Button).Tag as Tuple<PhraseCBO, PhraseCBO>), () => PopUpPhraseDetail.IsOpen = false);

        //        await Task.Delay(500);
        //        PopUpPhraseDetail.StaysOpen = false;
        //    }
        //}

        private void CheckResult()
        {
            if (answShowed)
            {
                InitModule();
                return;
            }

            answShowed = true;

            for (int i = 0; i < wPanelMain.Children.Count; i++)
            {
                ToggleButton tBtn = (wPanelMain.Children[i] as ToggleButton);

                Run showSentenceRun = (tBtn.Content as TextBlock).Inlines.First() as Run;
                showSentenceRun.TextDecorations.Add(TextDecorations.Underline);
                showSentenceRun.FontWeight = FontWeights.SemiBold;
                showSentenceRun.Cursor = Cursors.Hand;
                showSentenceRun.MouseDown += RunContent_MouseDown;

                if (tBtn != null)
                {
                    if (((bool)tBtn.Tag) == true) // iba Correct phrases
                    {
                        if (tBtn.IsChecked == true)
                        {
                            tBtn.ToBackgroundAnim(Colors.Green, 500);
                            ScorePanel.SaveResult(1);
                        }
                        else
                        {
                            tBtn.ToBackgroundAnim(Colors.Red, 500);
                        }
                    }
                    else
                    {
                        // FadeInThemeAnimation
                        tBtn.FadeAnim(1, 0.3, 500);
                    }

                }
            }

        }

    }
}

