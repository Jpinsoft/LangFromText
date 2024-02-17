using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.ContainerStorage.Types;
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
        ISmartStorage<LangModuleDataItemCBO> moduleStorage;

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

        public void LoadData(string moduleName)
        {
            List<Tuple<string, DateTime, int>> data = new List<Tuple<string, DateTime, int>>();

            moduleStorage = FEContext.ModulesRepository[moduleName];
            List<SmartData<LangModuleDataItemCBO>> scoreData = moduleStorage.SearchSmartData(s => true);

            foreach (SmartData<LangModuleDataItemCBO> dayScore in scoreData)
            {
                data.Add(new Tuple<string, DateTime, int>(moduleStorage.KeyName, dayScore.Created, dayScore.DataObject.Score));
            }

            TScoreChart.DataSource = data;
            TScoreChart.Series["SeriesScoreTimeLine"].XValueMember = "Item2";
            TScoreChart.Series["SeriesScoreTimeLine"].YValueMembers = "Item3";
            TScoreChart.DataBind();

            LabelScore.Content = string.Format(Properties.Resources.T021, scoreData.Sum(s => s.DataObject.Score));
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
                moduleStorage.ResetStorage();
                this.Close();
                FEContext.MNavigator.ShowStartPage();
            }
        }
    }
}
