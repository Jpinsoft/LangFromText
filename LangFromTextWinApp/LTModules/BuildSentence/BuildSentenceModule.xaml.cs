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
        List<WordCBO> targetWords = null;
        private const int CN_MIN_RATING = 5;
        static Random rnd = new Random();

        public BuildSentenceModule()
        {
            InitializeComponent();
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
            panelWords.Children.Clear();
            answShowed = false;

            PhraseCBO phrase = FEContext.LangFromText.GetRandomSentences(1, 4, 8).First();
            BtnOK.Content = phrase.ToString();
            targetWords = phrase.Words.Shuffle<WordCBO>(rnd).ToList();

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
                };

                ctrl.MouseMove += Ctrl_MouseMove;
                panelWords.Children.Add(ctrl);
            }

            // await Task.Delay((int)moveDoubleAnimation.Duration.TimeSpan.TotalMilliseconds);
            panelWords.Children[0].Focus();
        }

        private void Ctrl_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (sender is Control && e.LeftButton == MouseButtonState.Pressed)
            {
                // Package the data.
                DataObject data = new DataObject();
                data.SetData(typeof(Control), sender);
                Console.WriteLine("VYKONAVAM Ctrl_MouseMove");

                // Initiate the drag-and-drop operation.
                DragDrop.DoDragDrop((Control)sender, data, DragDropEffects.Move);
            }
        }

        private void SliderLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {

        }

        private void panelAnswer_Drop(object sender, DragEventArgs e)
        {
            var s = e.Source;

            Control labelWord = e.Data.GetData(typeof(Control)) as Control;

            if (labelWord != null)
            {
                panelWords.Children.Remove(labelWord);
                panelAnswer.Children.Add((Control)labelWord);
            }
        }
    }
}
