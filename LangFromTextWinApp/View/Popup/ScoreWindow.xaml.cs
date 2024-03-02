using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.ContainerStorage.Types;
using LangFromTextWinApp.Controls;
using LangFromTextWinApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;


namespace LangFromTextWinApp.View.Popup
{
    /// <summary>
    /// Interaction logic for ScoreView.xaml
    /// </summary>
    public partial class ScoreWindow : Window
    {
        private ScorePanelUserControl scorePanelUserControl;

        public ScoreWindow()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // cbGraphtType.ItemsSource = Enum.GetNames(typeof(SeriesChartType));
            cbGraphtType.ItemsSource = new SeriesChartType[] { SeriesChartType.Line, SeriesChartType.Bubble, SeriesChartType.Spline, SeriesChartType.FastLine, SeriesChartType.Column, SeriesChartType.StackedColumn, SeriesChartType.Area, SeriesChartType.StackedArea, SeriesChartType.Range, SeriesChartType.RangeColumn };
            cbGraphtType.SelectedItem = SeriesChartType.StackedColumn;
        }

        public void LoadData(ScorePanelUserControl scorePanelUserControl)
        {
            this.scorePanelUserControl = scorePanelUserControl;
            this.Title = $"Score detail";

            List<Tuple<string, DateTime, int>> data = new List<Tuple<string, DateTime, int>>();

            foreach (SmartData<LangModuleDataItemCBO> dayScore in scorePanelUserControl.LevelData)
            {
                if (dayScore.DataObject.Score > 0)
                    data.Add(new Tuple<string, DateTime, int>(scorePanelUserControl.ScoreStorage.KeyName, dayScore.Created, dayScore.DataObject.Score));
            }

            TScoreChart.DataSource = data;
            TScoreChart.Series["SeriesScoreTimeLine"].XValueMember = "Item2";
            TScoreChart.Series["SeriesScoreTimeLine"].YValueMembers = "Item3";

            TScoreChart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            TScoreChart.ChartAreas[0].AxisY.MajorGrid.Enabled = true;

            TScoreChart.DataBind();

            LabelScore.Content = $"{scorePanelUserControl.ModuleTitle} module";
            LabelScore2.Content = string.Format(Properties.Resources.T021, scorePanelUserControl.LevelData.Sum(s => s.DataObject.Score));
        }

        private void cbGraphtType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TScoreChart.Series.First().ChartType = (SeriesChartType)cbGraphtType.SelectedItem;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void BtnResetScoreData_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxWPF.ShowWarning(this, MessageBoxButton.OKCancel, Properties.Resources.T090) == true)
            {
                scorePanelUserControl.ScoreStorage.ResetStorage();
                this.Close();
                FEContext.MNavigator.ShowStartPage();
            }
        }
    }
}
