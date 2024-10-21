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
        bool answShowed = false;
        PhraseCBO targetPhrase = null;
        List<Label> controls = new List<Label>();
        private const int CN_MIN_RATING = 5;
        static Random rnd = new Random();
        private const int CN_PRE_INIT_DELAY = 700;
        AnimSuccesFail animExtender;

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
            ScorePanel.InitScorePanel(nameof(BuildSentenceModule), Properties.Resources.T202, (int)SliderLevel.Value);

            // Generate GUI
            controls.Clear();
            panelWords.Children.Clear();
            panelAnswer.Children.Clear();
            LabelCorrectAnsw.Visibility = Visibility.Hidden;

            answShowed = false;
            int levelVal = (int)(SliderLevel.Value) + 2; // 3, 4, 5

            targetPhrase = FEContext.LangFromText.GetRandomSentences(1, levelVal, 2 + levelVal).First();
            TxbCorrectAnsw.Text = targetPhrase.ToString();

            List<WordCBO> targetWords = targetPhrase.Words.ToList();
            targetWords.Shuffle<WordCBO>(rnd).ToList();

            //DoubleAnimation moveDoubleAnimation = Application.Current.FindResource("SelectPhraseModuleAnimation") as DoubleAnimation;
            //SolidColorBrush pallete2 = Application.Current.FindResource("Pallete2") as SolidColorBrush;
            //SolidColorBrush pallete3 = Application.Current.FindResource("Pallete3") as SolidColorBrush;

            for (int i = 0; i < targetWords.Count; i++)
            {
                WordCBO targetWord = targetWords[i];

                Control ctrl = new Label
                {
                    Name = "lblChar" + i,
                    Content = targetWord.ToString(),
                    Style = Application.Current.FindResource("WordLabelStyle") as Style,
                    Width = 250,
                    AllowDrop = true,
                    Tag = targetWord,
                };

                ctrl.MouseDoubleClick += Ctrl_MouseDoubleClick;
                ctrl.MouseMove += Ctrl_MouseMove;
                panelWords.Children.Add(ctrl);

                Label targetBorder = new Label
                {
                    AllowDrop = true,
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Gray,
                    Margin = new Thickness(10),
                    Padding = new Thickness(2),
                    Height = 80
                };

                targetBorder.Drop += Border_Drop;
                panelAnswer.Children.Add(targetBorder);
            }

            // await Task.Delay((int)moveDoubleAnimation.Duration.TimeSpan.TotalMilliseconds);
            panelWords.Children[0].Focus();
        }

        private void Ctrl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label wordLabel = sender as Label;

            if (wordLabel != null && panelWords.Children.Contains(wordLabel))
            {
                foreach (object item in panelAnswer.Children)
                {
                    Label borderLabel = item as Label;

                    if (borderLabel != null && borderLabel.Content == null)
                    {
                        panelWords.Children.Remove(wordLabel);
                        borderLabel.Content = wordLabel;
                        controls.Add((Label)sender);
                        break;
                    }
                }

                CheckAnsw();
            }
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            Control labelWord = e.Data.GetData(typeof(Control)) as Control;

            if (labelWord != null)
            {
                panelWords.Children.Remove(labelWord);
                ((Label)sender).Content = (Control)labelWord;
                controls.Add((Label)sender);
            }
        }

        private void Ctrl_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (sender is Label && e.LeftButton == MouseButtonState.Pressed)
            {
                // Package the data.
                DataObject data = new DataObject();
                data.SetData(typeof(Control), sender);
                // Console.WriteLine("VYKONAVAM Ctrl_MouseMove");

                // Initiate the drag-and-drop operation.
                DragDrop.DoDragDrop((Label)sender, data, DragDropEffects.Move);

                CheckAnsw();
            }
        }

        private void CheckAnsw()
        {
            if (controls.Count == targetPhrase.Words.Count)
            {
                bool res = true;

                for (int i = 0; i < controls.Count; i++)
                {
                    if ((WordCBO)controls[i].Tag != targetPhrase.Words[i])
                        res = false;
                }

                LabelCorrectAnsw.Visibility = Visibility.Visible;

                if (res)
                    Success();
                else
                    Fail();
            }
        }


        private void Success()
        {
            ScorePanel.UpdateScore(1);
            animExtender.AnimSuccess();
            // await Task.Delay(CN_PRE_INIT_DELAY * 3);
            FAIcon.Foreground = Brushes.Green;
            FAIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.CheckCircle;
        }

        private void Fail()
        {
            animExtender.AnimFail();
            // await Task.Delay(CN_PRE_INIT_DELAY * 3);
            FAIcon.Foreground = Brushes.Red;
            FAIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.Ban;
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
            InitModule();
        }

        private async void TxbCorrectAnsw_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // TODo SEt TAG
            if ((sender as Run)?.Tag as Tuple<PhraseCBO, PhraseCBO> != null)
            {
                PopUpPhraseDetail.StaysOpen = true;
                PopUpPhraseDetail.IsOpen = true;
                UserControlPhraseDetail.Init(((sender as Run).Tag as Tuple<PhraseCBO, PhraseCBO>), () => PopUpPhraseDetail.IsOpen = false);

                await Task.Delay(500);
                PopUpPhraseDetail.StaysOpen = false;
            }
        }
    }
}
