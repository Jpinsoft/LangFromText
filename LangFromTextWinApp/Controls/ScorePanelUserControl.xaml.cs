using Jpinsoft.CompactStorage.Types;
using Jpinsoft.LangTainer.CBO;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.View.Popup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LangFromTextWinApp.Controls
{
    /// <summary>
    /// Interaction logic for ScorePanelUserControl.xaml
    /// </summary>
    public partial class ScorePanelUserControl : UserControl
    {
        public ICompactStorage<LangModuleDataItemCBO> ScoreStorage { get; private set; }
        public List<LangModuleDataItemCBO> LevelData { get; private set; }
        public string KeyPrefix { get { return $"Level{Level}-"; } }
        public string ModuleTitle { get; private set; }
        public int Level { get; private set; }

        public ScorePanelUserControl()
        {
            InitializeComponent();
        }

        public void InitScorePanel(string moduleName, string moduleTitle, int level)
        {
            this.Level = level;
            this.ModuleTitle = moduleTitle;

            ScoreStorage = FEContext.ModulesRepository[moduleName];

            TxbScoreToDay.Text = $"{GetScoreToday().Score}";

            LevelData = ScoreStorage.FilterAsList(_ => _.Key.StartsWith(KeyPrefix));
            TxbScoreTotal.Text = $"{LevelData.Sum(item => item.Score)}";
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

        public LangModuleDataItemCBO GetScoreToday()
        {
            string dataKey = KeyPrefix + DateTime.Now.ToString("yyyy-MM-dd");
            LangModuleDataItemCBO sData = ScoreStorage[dataKey];

            if (sData == null)
            {
                ScoreStorage[dataKey] = new LangModuleDataItemCBO();
                sData = ScoreStorage[dataKey];
            }

            return sData;
        }

        public void UpdateScore(int score)
        {
            LangModuleDataItemCBO scoreSmartData = GetScoreToday();
            scoreSmartData.Score += score;

            // TODo Save score
            ScoreStorage[scoreSmartData.Key] = scoreSmartData;
        }
    }
}
