using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.Utils;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.LTModules.SelectWord;
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
            // ScorePanel.InitScorePanel(nameof(SelectWordModule), Properties.Resources.T202, (int)SliderLevel.Value);
            // Generate GUI
            controls.Clear();
            panelWords.Children.Clear();
            panelAnswer.Children.Clear();
            LabelCorrectAnsw.Visibility = Visibility.Hidden;

            answShowed = false;

            targetPhrase = FEContext.LangFromText.GetRandomSentences(1, 4, 8).First();
            BtnOK.Content = targetPhrase.ToString();
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
                    Tag = targetWord
                };

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

                ctrl.MouseMove += Ctrl_MouseMove;
                panelWords.Children.Add(ctrl);

                panelAnswer.Children.Add(targetBorder);
            }

            // await Task.Delay((int)moveDoubleAnimation.Duration.TimeSpan.TotalMilliseconds);
            panelWords.Children[0].Focus();
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            Control labelWord = e.Data.GetData(typeof(Control)) as Control;

            if (labelWord != null)
            {
                panelWords.Children.Remove(labelWord);
                ((Label)sender).Content = (Control)labelWord;
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
                Console.WriteLine("VYKONAVAM Ctrl_MouseMove");

                // Initiate the drag-and-drop operation.
                DragDrop.DoDragDrop((Label)sender, data, DragDropEffects.Move);
                controls.Add((Label)sender);

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

        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
