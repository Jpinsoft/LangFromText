using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.Utils;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.LTModules.SelectWord;
using LangFromTextWinApp.Properties;
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
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LangFromTextWinApp.LTModules.BuildSentence
{
    /// <summary>
    /// Interaction logic for BuildSentenceModule.xaml
    /// </summary>
    public partial class BuildSentenceModule : UserControl, ILTModuleView
    {
        bool answShown = false;
        PhraseCBO targetPhrase = null;
        private const int CN_MIN_RATING = 5;
        static Random rnd = new Random();
        private const int CN_PRE_INIT_DELAY = 700;
        AnimSuccesFail animExtender;

        public List<Label> AnswBorders
        {
            get
            {
                List<Label> res = new List<Label>();

                foreach (Label label in panelAnswer.Children)
                    res.Add(label);

                return res;
            }
        }

        public BuildSentenceModule()
        {
            InitializeComponent();

            animExtender = new AnimSuccesFail(this.LabelCorrectAnsw, CN_PRE_INIT_DELAY, false);
            SliderLevel.Value = Properties.Settings.Default.BuildSentenceModuleLevel;
        }

        public string ModuleName { get { return nameof(SelectWordModule); } }

        public void ShowModule()
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitModule();
        }

        private void InitModule()
        {
            DoubleAnimation moveDoubleAnimation = Application.Current.FindResource(FEConstants.RESKEY_SelectPhraseModuleAnimation) as DoubleAnimation;
            SolidColorBrush pallete2 = Application.Current.FindResource(FEConstants.RESKEY_Pallete2) as SolidColorBrush;
            SolidColorBrush pallete3 = Application.Current.FindResource(FEConstants.RESKEY_Pallete3) as SolidColorBrush;

            ScorePanel.InitScorePanel(nameof(BuildSentenceModule), Properties.Resources.T202, (int)SliderLevel.Value);

            // Generate GUI
            answShown = true;
            panelWords.Children.Clear();
            panelAnswer.Children.Clear();
            LabelCorrectAnsw.Visibility = Visibility.Hidden;

            answShown = false;
            int levelVal = (int)(SliderLevel.Value) + 2; // 3, 4, 5

            targetPhrase = FEContext.LangFromText.GetRandomSentences(1, levelVal, 2 + levelVal).First();

            TxbCorrectAnsw.Text = (char.ToUpper(targetPhrase.ToString()[0]) + targetPhrase.ToString().Substring(1));
            TxbCorrectAnsw.Tag = new Tuple<PhraseCBO, PhraseCBO>(targetPhrase, targetPhrase);

            List<WordCBO> targetWords = targetPhrase.Words.ToList();
            targetWords.Shuffle<WordCBO>(rnd).ToList();

            //DoubleAnimation moveDoubleAnimation = Application.Current.FindResource("SelectPhraseModuleAnimation") as DoubleAnimation;
            //SolidColorBrush pallete2 = Application.Current.FindResource("Pallete2") as SolidColorBrush;
            //SolidColorBrush pallete3 = Application.Current.FindResource("Pallete3") as SolidColorBrush;

            for (int i = 0; i < targetWords.Count; i++)
            {
                WordCBO targetWord = targetWords[i];

                Label ctrl = new Label
                {
                    Name = "lblChar" + i,
                    Content = targetWord.ToString(),
                    Style = Application.Current.FindResource("WordLabelStyle") as Style,
                    Width = 250,
                    AllowDrop = true,
                    Tag = targetWord,
                    Margin = new Thickness(11)
                };

                if ((i % 2) != 0)
                {
                    ctrl.AnimTranslateX(-moveDoubleAnimation.From.Value * (rnd.Next(0, 30) / 100f), 0, moveDoubleAnimation.EasingFunction, moveDoubleAnimation.Duration.TimeSpan.TotalMilliseconds);
                }
                else
                    ctrl.AnimTranslateX(moveDoubleAnimation.From.Value * (rnd.Next(0, 30) / 100f), 0, moveDoubleAnimation.EasingFunction, moveDoubleAnimation.Duration.TimeSpan.TotalMilliseconds);

                ctrl.MouseMove += Ctrl_MouseMove;
                ctrl.Cursor = Cursors.Hand;
                ctrl.MouseUp += Ctrl_MouseUp;
                panelWords.Children.Add(ctrl);

                Label targetBorder = new Label
                {
                    AllowDrop = true,

                    Background = new SolidColorBrush(Color.FromArgb(64, 128, 128, 128)),
                    Margin = new Thickness(20, 5, 20, 5),
                    Padding = new Thickness(0),
                    Height = 60,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };

                targetBorder.Drop += TatgetBorder_Drop;
                targetBorder.DragOver += TargetBorder_DragOver;
                panelAnswer.Children.Add(targetBorder);
            }

            BtnOK.Focus();
        }

        private void Ctrl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!answShown && sender is Label && e.LeftButton == MouseButtonState.Released)
            {
                Label wordLabel = sender as Label;

                if (wordLabel != null)
                {
                    // Move to panelAnser
                    if (panelWords.Children.Contains(wordLabel))
                    {
                        foreach (object item in panelAnswer.Children)
                        {
                            Label borderLabel = item as Label;

                            if (borderLabel != null && borderLabel.Content == null)
                            {
                                wordLabel.Margin = new Thickness(5);
                                panelWords.Children.Remove(wordLabel);
                                borderLabel.Content = wordLabel;
                                break;
                            }
                        }
                    }
                    else
                    {  // Remove from panelAnser
                        foreach (object item in panelAnswer.Children)
                        {
                            Label borderLabel = item as Label;

                            if (borderLabel != null && borderLabel.Content == wordLabel)
                            {
                                wordLabel.Margin = new Thickness(11);
                                borderLabel.Content = null;
                                panelWords.Children.Add(wordLabel);
                            }
                        }
                    }
                }
            }
        }

        private void TargetBorder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = (((Label)sender)?.Content == null) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void TatgetBorder_Drop(object sender, DragEventArgs e)
        {
            Control labelWord = e.Data.GetData(typeof(Control)) as Control;

            if (labelWord != null)
            {
                if (((Label)sender).Content == null)
                {
                    panelWords.Children.Remove(labelWord);

                    foreach (Label lblAnsw in AnswBorders)
                    {
                        if (lblAnsw.Content == labelWord)
                            lblAnsw.Content = null;
                    }


                    labelWord.Margin = new Thickness(5);
                    ((Label)sender).Content = labelWord;
                }
            }
        }

        private void Ctrl_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!answShown && sender is Label && e.LeftButton == MouseButtonState.Pressed)
            {
                // Package the data.
                DataObject data = new DataObject();
                data.SetData(typeof(Control), sender);
                // Console.WriteLine("VYKONAVAM Ctrl_MouseMove");

                // Initiate the drag-and-drop operation.
                DragDrop.DoDragDrop((Label)sender, data, DragDropEffects.All);
            }
        }

        private void CheckAnsw()
        {
            if (answShown)
            {
                InitModule();
                return;
            }

            answShown = true;
            bool res = false;

            if (AnswBorders.Count(_ => _.Content != null) == targetPhrase.Words.Count)
            {
                res = true;

                for (int i = 0; i < AnswBorders.Count(); i++)
                {
                    WordCBO answWord = ((Label)AnswBorders[i].Content).Tag as WordCBO;

                    if (answWord != targetPhrase.Words[i])
                        res = false;
                }
            }

            if (res)
                Success();
            else
                Fail();
        }

        private void Success()
        {
            ScorePanel.UpdateScore(1);
            animExtender.AnimSuccess();
            // await Task.Delay(CN_PRE_INIT_DELAY * 3);
            FAIcon.Foreground = Brushes.Green;
            FAIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.CheckCircle;
            LabelCorrectAnsw.Visibility = Visibility.Visible;
        }

        private void Fail()
        {
            animExtender.AnimFail();
            // await Task.Delay(CN_PRE_INIT_DELAY * 3);
            FAIcon.Foreground = Brushes.Red;
            FAIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.Ban;
            LabelCorrectAnsw.Visibility = Visibility.Visible;
        }

        private void SliderLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.IsLoaded)
            {
                InitModule();

                Settings.Default.BuildSentenceModuleLevel = (int)SliderLevel.Value;
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            CheckAnsw();
        }

        private async void TxbCorrectAnsw_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // TODo SET TAG
            if ((sender as Run)?.Tag as Tuple<PhraseCBO, PhraseCBO> != null)
            {
                PopUpPhraseDetail.StaysOpen = true;
                PopUpPhraseDetail.IsOpen = true;
                UserControlPhraseDetail.Init(((sender as Run).Tag as Tuple<PhraseCBO, PhraseCBO>), () => PopUpPhraseDetail.IsOpen = false);

                await Task.Delay(500);
                PopUpPhraseDetail.StaysOpen = false;
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CheckAnsw();
                e.Handled = true;
            }
        }
    }
}
