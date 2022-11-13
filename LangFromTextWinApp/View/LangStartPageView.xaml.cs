using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.ContainerStorage.Types;
using Jpinsoft.LangTainer.Types;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LangFromTextWinApp.View
{
    /// <summary>
    /// Interaction logic for LangStartPageView.xaml
    /// </summary>
    public partial class LangStartPageView : UserControl
    {
        public LangStartPageViewModel ViewModel { get { return this.DataContext as LangStartPageViewModel; } }

        public LangStartPageView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new LangStartPageViewModel();

            if (!ViewModel.IsDataModelEmpty)
            {
                await LoadDBGraph();
            }
        }

        private async Task LoadDBGraph()
        {
            List<Tuple<string, int>> data = new List<Tuple<string, int>>();
            data.Add(new Tuple<string, int>("WordsCount", ViewModel.WordsCount));
            data.Add(new Tuple<string, int>("SentencesCount", ViewModel.SentencesCount));
            data.Add(new Tuple<string, int>("TextSourcesCount", ViewModel.TextSourcesCount));

            TStatusChart.DataSource = data;
            TStatusChart.Series["SeriesStatus"].XValueMember = "Item1";
            TStatusChart.Series["SeriesStatus"].YValueMembers = "Item2";

            TStatusChart.ChartAreas[0].Area3DStyle.Enable3D = true;
            TStatusChart.ChartAreas[0].Area3DStyle.Rotation = -120;

            TStatusChart.Legends[0].Font = new System.Drawing.Font(TStatusChart.Legends[0].Font.FontFamily, 13, System.Drawing.FontStyle.Italic);

            int maxVal = Math.Max(ViewModel.WordsCount, ViewModel.SentencesCount);
            maxVal = Math.Max(maxVal, ViewModel.TextSourcesCount);


            for (int i = 0; i < 60; i++)
            {
                TStatusChart.ChartAreas[0].Area3DStyle.Rotation += 2;
                await Task.Delay(20);
                TStatusChart.DataBind();
            }

            // One Module Data
            //for (int i = 0; i < maxVal; i++)
            //{
            //    if (i <= ViewModel.WordsCount)
            //        data[0] = new Tuple<string, int>("WordsCount", i);

            //    if (i <= ViewModel.SentencesCount)
            //        data[1] = new Tuple<string, int>("SentencesCount", i);

            //    if (i <= ViewModel.TextSourcesCount)
            //        data[2] = new Tuple<string, int>("TextSourcesCount", i);

            //    if (i % 50 == 0)
            //    {
            //        TStatusChart.ChartAreas[0].Area3DStyle.Rotation++;
            //        await Task.Delay(30);
            //        TStatusChart.DataBind();
            //    }
            //}
        }

        private void LinkOpenPage_Click(object sender, RoutedEventArgs e)
        {
            string commandParam = null;

            switch (sender)
            {
                case Hyperlink senderAsHyperlink:
                    commandParam = senderAsHyperlink.CommandParameter as string;
                    break;

                case Button senderAsButton:
                    commandParam = senderAsButton.CommandParameter as string;
                    break;

                default:
                    throw new NotImplementedException($"LinkOpenPage_Click does not implement type {sender.GetType()}");
            }

            if (!string.IsNullOrEmpty(commandParam))
            {
                FEContext.MNavigator.ShowPageByViewName(commandParam);
            }
        }
    }
}
