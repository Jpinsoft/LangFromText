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

namespace LangFromTextWinApp.LTModules.SelectWord
{
    /// <summary>
    /// Interaction logic for SelectWordModule.xaml
    /// </summary>
    public partial class SelectWordModule : UserControl, ILTModuleView
    {
        static Random rnd = new Random();
        List<WordCBO> targetWords = null;
        private const int CN_MIN_RATING = 5;
        private const int CN_PRE_INIT_DELAY = 700;
        bool answShowed = false;
        ISmartStorage<LangModuleDataItemCBO> storage;

        public SelectWordModule()
        {
            InitializeComponent();
        }

        #region ILTModuleView

        public string ModuleName { get { return nameof(SelectWordModule); } }

        public void ShowModule()
        {
        }

        #endregion

        #region Events

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CheckResult();
                e.Handled = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitModule();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            CheckResult();
        }

        private void SliderLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        #endregion

        private async void InitModule()
        {
            storage = FEContext.ModulesRepository[nameof(SelectWordModule)];
            ScorePanel.InitScorePanel(nameof(SelectWordModule));

            // Generate GUI
            wPanelMain.Children.Clear();
            answShowed = false;

            //List<WordCBO> words = MainWindow.LangFromText.GetRandomWords(10);

            targetWords = FEContext.LangFromText.GetRandomWords(6, CN_MIN_RATING, 6);
            IList<int> rndCharsIndex = RandomTools.Shuffle<int>(RandomTools.GeneratePostupnost<int>(0, targetWords.Count - 1), rnd);
            rndCharsIndex = rndCharsIndex.Take((int)SliderLevel.Value).ToList();

            DoubleAnimation moveDoubleAnimation = Application.Current.FindResource("SelectPhraseModuleAnimation") as DoubleAnimation;
            SolidColorBrush pallete2 = Application.Current.FindResource("Pallete2") as SolidColorBrush;
            SolidColorBrush pallete3 = Application.Current.FindResource("Pallete3") as SolidColorBrush;

            for (int i = 0; i < targetWords.Count; i++)
            {
                WordCBO word = targetWords[i];
                bool isCorrectWord = true;

                if (!rndCharsIndex.Contains(i))
                {
                    targetWords[i] = null;

                    StringBuilder sb = new StringBuilder().Append(word.Value.ToCharArray().Shuffle(rnd).ToArray());

                    word = new WordCBO { ID = -1, Value = sb.ToString() };
                    isCorrectWord = false;
                }

                ToggleButton tButton = new ToggleButton { Background = pallete2, Name = "ToggleButton" + (i + 1), Tag = isCorrectWord, Content = word, Style = Application.Current.FindResource("PhraseToggleButtonStyle") as Style };
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

        private void CheckResult()
        {
            if (answShowed)
            {
                InitModule();
                return;
            }

            answShowed = true;
            bool totalResult = true;

            for (int i = 0; i < wPanelMain.Children.Count; i++)
            {
                ToggleButton tBtn = (wPanelMain.Children[i] as ToggleButton);

                if (tBtn != null)
                {
                    if (((bool)tBtn.Tag) == true) // iba Correct word
                    {
                        Run runControl = new Run(tBtn.Content.ToString());
                        tBtn.Content = runControl;
                        tBtn.Cursor = Cursors.Hand;
                        runControl.TextDecorations.Add(TextDecorations.Underline);
                        runControl.MouseDown += RunControl_MouseDown;
                        runControl.ToolTip = Properties.Resources.T089;

                        if (tBtn.IsChecked == true)
                        {
                            tBtn.ToBackgroundAnim(Colors.Green, 500);
                        }
                        else
                        {
                            tBtn.ToBackgroundAnim(Colors.Red, 500);
                            totalResult = false;
                        }
                    }
                    else // Zle slovo
                    {
                        if (tBtn.IsChecked == true)
                        {
                            tBtn.ToBackgroundAnim(Colors.Red, 500);
                            totalResult = false;
                        }

                        // FadeInThemeAnimation
                        tBtn.FadeAnim(1, 0.3, 500);
                    }
                }
            }

            if (totalResult)
            {
                ScorePanel.SaveResult((int)SliderLevel.Value);
                // TODO Success anim
            }
        }

        private void RunControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WPFHelpers.OpenTranslator(((Run)sender).Text);
        }
    }
}
