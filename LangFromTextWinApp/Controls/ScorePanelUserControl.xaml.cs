using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.ContainerStorage.Types;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.View;
using LangFromTextWinApp.View.Popup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LangFromTextWinApp.Controls
{
    /// <summary>
    /// Interaction logic for ScorePanelUserControl.xaml
    /// </summary>
    public partial class ScorePanelUserControl : UserControl
    {
        private string moduleName;
        ISmartStorage<LangModuleDataItemCBO> storage;

        public ScorePanelUserControl()
        {
            InitializeComponent();
        }

        public void InitScorePanel(string moduleName)
        {
            this.moduleName = moduleName;
            storage = FEContext.ModulesRepository[moduleName];

            TxbScoreToDay.Text = $"{GetScoreToday().DataObject.Score}";

            List<SmartData<LangModuleDataItemCBO>> allModuleData = storage.SearchSmartData();
            TxbScoreTotal.Text = $"{allModuleData.Sum( item => item.DataObject.Score)}";
        }

        private void LinkScore_Click(object sender, RoutedEventArgs e)
        {
            ScoreWindow scoreView = new ScoreWindow();
            scoreView.Owner = FEContext.MainWin;

            scoreView.Width = scoreView.Owner.ActualWidth - scoreView.Owner.ActualWidth * 0.25f;
            scoreView.Height = scoreView.Owner.ActualHeight - scoreView.Owner.ActualHeight * 0.25f;

            scoreView.LoadData(moduleName);
            scoreView.ShowDialog();
        }

        private SmartData<LangModuleDataItemCBO> GetScoreToday()
        {
            string dataKey = DateTime.Now.ToString("yyyy-MM-dd");
            SmartData<LangModuleDataItemCBO> sData = storage.GetSmartData(dataKey);

            if (sData == null)
            {
                storage.SetSmartData(new LangModuleDataItemCBO(), dataKey);
                sData = storage.GetSmartData(dataKey);
            }

            if (sData.DataObject == null)
                sData.DataObject = new LangModuleDataItemCBO();

            return sData;
        }

        public void SaveResult(int score)
        {
            SmartData<LangModuleDataItemCBO> scoreSmartData = GetScoreToday();
            scoreSmartData.DataObject.Score += score;

            // TODo Save score
            storage.SetSmartData(scoreSmartData.DataObject, scoreSmartData.Key);
        }
    }
}
