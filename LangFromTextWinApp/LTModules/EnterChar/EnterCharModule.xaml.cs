using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.ContainerStorage.Types;
using Jpinsoft.LangTainer.Utils;
using LangFromTextWinApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LangFromTextWinApp.LTModules.EnterChar
{
    /// <summary>
    /// Interaction logic for EnterCharModule.xaml
    /// </summary>
    public partial class EnterCharModule : UserControl, ILTModuleView
    {
        static Random rnd = new Random();
        WordCBO targetWord = null;
        private const int CN_MIN_RATING = 5;
        private const int CN_PRE_INIT_DELAY = 700;
        List<TextBox> textBoxesQues = new List<TextBox>();
        AnimSuccesFail animExtender;
        bool answShowed = false;
        ISmartStorage<LangModuleDataItemCBO> storage;

        public EnterCharModule()
        {
            InitializeComponent();
            animExtender = new AnimSuccesFail(this.LabelCorrectAnsw, CN_PRE_INIT_DELAY, false);
        }

        #region Events

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitModule();
        }

        private void Txb_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Focus on NEXT TextBox
            int txbIndex = textBoxesQues.IndexOf(sender as TextBox);

            if (txbIndex + 1 < textBoxesQues.Count)
                textBoxesQues[txbIndex + 1].Focus();
        }

        private void Txb_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txb = (sender as TextBox);

            if (txb != null)
            {
                txb.TextChanged -= Txb_TextChanged;

                //txb.IsReadOnly = false;
                if (txb.Text == "?")
                    txb.Text = string.Empty;
                else
                    txb.Select(0, txb.Text.Length);

                txb.TextChanged += Txb_TextChanged;
            }
        }

        private void Txb_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txb = (sender as TextBox);

            if (txb != null)
            {
                //txb.IsReadOnly = true;
                txb.Text = string.IsNullOrEmpty(txb.Text) ? "?" : txb.Text;
            }
        }

        private void SliderLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.IsLoaded)
                InitModule();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CheckResult();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            CheckResult();
        }

        #endregion

        #region ILTModuleView

        public string ModuleName { get { return nameof(EnterCharModule); } }

        public void ShowModule()
        {
        }

        #endregion

        private async void InitModule()
        {
            storage = FEContext.ModulesRepository[nameof(EnterCharModule)];
            ScorePanel.InitScorePanel(nameof(EnterCharModule));
            targetWord = FEContext.LangFromText.GetRandomWords(1, CN_MIN_RATING, 6).FirstOrDefault();
            TxbCorrectAnsw.Text = targetWord.Value;
            IList<int> rndCharsIndex = RandomTools.Shuffle<int>(RandomTools.GeneratePostupnost<int>(0, targetWord.Value.Length - 1), rnd);
            rndCharsIndex = rndCharsIndex.Take((int)SliderLevel.Value).ToList();

            // Generate GUI
            WrapPanelChars.Children.Clear();
            textBoxesQues.Clear();
            LabelCorrectAnsw.Visibility = Visibility.Hidden;
            answShowed = false;
            DoubleAnimation moveDoubleAnimation = Application.Current.FindResource("EnterCharModuleAnimation") as DoubleAnimation;

            for (int i = 0; i < targetWord.Value.Length; i++)
            {
                Control ctrl = new Label
                {
                    Name = "txbChar" + i,
                    Content = targetWord.Value[i].ToString(),
                    Style = Application.Current.FindResource("WordLabelStyle") as Style
                };

                if (rndCharsIndex.Contains(i))
                {
                    TextBox txb = new TextBox
                    {
                        Name = "txbChar" + i,
                        Text = "?",
                        MaxLength = 1,
                        Style = Application.Current.FindResource("WordTextBoxStyle") as Style
                    };

                    //txb.Focus();
                    //txb.Select(0, 1);
                    txb.LostFocus += Txb_LostFocus;
                    txb.GotFocus += Txb_GotFocus;
                    txb.TextChanged += Txb_TextChanged;
                    ctrl = txb;
                    textBoxesQues.Add(txb);
                }

                WrapPanelChars.Children.Add(ctrl);

                if ((i % 2) != 0)
                    ctrl.AnimTranslateY(-moveDoubleAnimation.From.Value * (rnd.Next(80, 100) / 100f), 0, moveDoubleAnimation.EasingFunction, moveDoubleAnimation.Duration.TimeSpan.TotalMilliseconds);
                else
                    ctrl.AnimTranslateY(moveDoubleAnimation.From.Value * (rnd.Next(80, 100) / 100f), 0, moveDoubleAnimation.EasingFunction, moveDoubleAnimation.Duration.TimeSpan.TotalMilliseconds);
            }

            await Task.Delay((int)moveDoubleAnimation.Duration.TimeSpan.TotalMilliseconds);
            textBoxesQues.First().Focus();
        }

        private void CheckResult()
        {
            if (answShowed)
            {
                InitModule();
                return;
            }

            answShowed = true;
            LabelCorrectAnsw.Visibility = Visibility.Visible;
            bool match = true;

            for (int i = 0; i < targetWord.Value.Length; i++)
            {
                TextBox txb = (WrapPanelChars.Children[i] as TextBox);

                if (txb != null && (string.IsNullOrEmpty(txb.Text) || string.Compare(targetWord.Value[i].ToString().RemoveDiacritics(), txb.Text[0].ToString().RemoveDiacritics(), true) != 0))
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                animExtender.AnimSuccess();
                // await Task.Delay(CN_PRE_INIT_DELAY * 3);
                FAIcon.Foreground = Brushes.Green;
                FAIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.CheckCircle;


                ScorePanel.SaveResult(1);
            }
            else
            {
                animExtender.AnimFail();
                // await Task.Delay(CN_PRE_INIT_DELAY * 3);
                FAIcon.Foreground = Brushes.Red;
                FAIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.Ban;
            }
        }

        private void TxbCorrectAnsw_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WPFHelpers.OpenTranslator(targetWord.Value);
        }
    }
}
