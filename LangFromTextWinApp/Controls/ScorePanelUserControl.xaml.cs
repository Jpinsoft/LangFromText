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
        public ISmartStorage<LangModuleDataItemCBO> ScoreStorage { get; private set; }
        public List<SmartData<LangModuleDataItemCBO>> LevelData { get; private set; }
        public string KeyPrefix { get { return $"Level{Level}-"; } }
        public int Level { get; private set; }

        public ScorePanelUserControl()
        {
            InitializeComponent();
        }

        public void InitScorePanel(string moduleName, int level)
        {
            this.Level = level;

            ScoreStorage = FEContext.ModulesRepository[moduleName];

            TxbScoreToDay.Text = $"{GetScoreToday().DataObject.Score}";

            LevelData = ScoreStorage.SearchSmartData(_ => _.Key.StartsWith(KeyPrefix));
            TxbScoreTotal.Text = $"{LevelData.Sum(item => item.DataObject.Score)}";
        }

        private void LinkScore_Click(object sender, RoutedEventArgs e)
        {
            ScoreWindow scoreView = new ScoreWindow();
            scoreView.Owner = FEContext.MainWin;

            scoreView.Width = scoreView.Owner.ActualWidth - scoreView.Owner.ActualWidth * 0.25f;
            scoreView.Height = scoreView.Owner.ActualHeight - scoreView.Owner.ActualHeight * 0.25f;

            scoreView.LoadData(this);
            scoreView.ShowDialog();
        }

        public SmartData<LangModuleDataItemCBO> GetScoreToday()
        {
            string dataKey = KeyPrefix + DateTime.Now.ToString("yyyy-MM-dd");
            SmartData<LangModuleDataItemCBO> sData = ScoreStorage.GetSmartData(dataKey);

            if (sData == null)
            {
                ScoreStorage.SetSmartData(new LangModuleDataItemCBO(), dataKey);
                sData = ScoreStorage.GetSmartData(dataKey);
            }

            if (sData.DataObject == null)
                sData.DataObject = new LangModuleDataItemCBO();

            return sData;
        }

        public void UpdateScore(int score)
        {
            SmartData<LangModuleDataItemCBO> scoreSmartData = GetScoreToday();
            scoreSmartData.DataObject.Score += score;

            // TODo Save score
            ScoreStorage.SetSmartData(scoreSmartData.DataObject, scoreSmartData.Key);
        }
    }
}
