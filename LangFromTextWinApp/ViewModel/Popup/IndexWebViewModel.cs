using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.LangAdapters;
using Jpinsoft.LangTainer.LangAdapters.Html;
using Jpinsoft.LangTainer.Types;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LangFromTextWinApp.ViewModel.Popup
{
    public class IndexWebViewModel : ViewModelBase
    {
        private const int CN_MAX_HISTORY_COUNT = 20;
        public bool IsWorking { get; set; } = false;
        ILangAdapter langAdapter;

        public ObservableCollection<string> ScanHistory { get; set; } = new ObservableCollection<string>();

        private string btnIndexWebText = Resources.T058;

        public string BtnIndexWebText
        {
            get { return btnIndexWebText; }
            set { SetProperty(ref btnIndexWebText, value); }
        }

        private ObservableCollection<Tuple<string, string, bool>> scanOutput = new ObservableCollection<Tuple<string, string, bool>>();

        public ObservableCollection<Tuple<string, string, bool>> ScanOutput
        {
            get { return scanOutput; }
            set { SetProperty(ref scanOutput, value); }
        }

        private int successURLCount;

        public int SuccessURLCount
        {
            get { return successURLCount; }
            set { SetProperty(ref successURLCount, value); }
        }

        private int errorURLCount;

        public int ErrorURLCount
        {
            get { return errorURLCount; }
            set { SetProperty(ref errorURLCount, value); }
        }

        private bool canIndex = false;

        public bool CanIndex
        {
            get { return canIndex; }
            set { SetProperty(ref canIndex, value); }
        }

        private string targetURL = Resources.T084;

        public string TargetURL
        {
            get { return targetURL; }
            set
            {
                SetProperty(ref targetURL, value);
                CanIndex = (string.IsNullOrEmpty(value) || !Uri.IsWellFormedUriString(value, UriKind.Absolute)) ? false : true;
            }
        }

        private int maxLimit;

        public IndexWebViewModel()
        {

        }

        public void LoadHistory()
        {
            if (Settings.Default.ScanHistory == null)
                Settings.Default.ScanHistory = new System.Collections.Specialized.StringCollection();

            ScanHistory.Clear();

            foreach (string historyItem in Settings.Default.ScanHistory)
                ScanHistory.Add(historyItem);

            // CbTargetURL.SelectedIndex = 0;
        }

        public void AddToScanHistory(string url)
        {
            url = url.ToLower();

            if (!Settings.Default.ScanHistory.Contains(url))
            {
                Settings.Default.ScanHistory.Insert(0, url);

                if (Settings.Default.ScanHistory.Count > CN_MAX_HISTORY_COUNT)
                    Settings.Default.ScanHistory.RemoveAt(Settings.Default.ScanHistory.Count - 1);

                LoadHistory();
            }
        }

        public void RemoveFromScanHistory(string url)
        {
            Settings.Default.ScanHistory.Remove(url);

            LoadHistory();
        }

        public async Task IndexWeb(int maxLevel, int maxLimit, string onlyURLContains, string onlyURLNotContains)
        {
            if (!IsWorking)
            {
                if (string.IsNullOrEmpty(TargetURL))
                    return;

                SuccessURLCount = 0;
                ErrorURLCount = 0;
                ScanOutput.Clear();
                this.maxLimit = maxLimit;

                IsWorking = true;
                BtnIndexWebText = Resources.T060;
                langAdapter = new HtmlLangAdapter();
                langAdapter.AdapterSettings = new HtmlLangAdapterSettings { HtmlScanLevel = maxLevel, AllowedURLContains = onlyURLContains, AllowedURLNotContains = onlyURLNotContains };

                List<TextSourceCBO> textSources = new List<TextSourceCBO>();
                string selectedUrl = TargetURL;
                Stopwatch sw = Stopwatch.StartNew();

                try
                {
                    await Task.Run(() =>
                    {
                        textSources = langAdapter.GetTextSources(selectedUrl, new Action<object>(OnProgressChanged));

                        foreach (TextSourceCBO item in textSources)
                        {
                            try
                            {
                                FEContext.LangFromText.IndexSource(item);
                            }
                            catch (Exception ex)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    ScanOutput.Insert(0, new Tuple<string, string, bool>(string.Empty, "--- " + ex.Message, true));
                                });
                            }
                        }
                    });

                    sw.Stop();

                    FEContext.LangFromText.SaveDatabase();
                    // nesmie byt v Await v inom vlakne
                    FEContext.MainWin.RefreshState();

                    ScanOutput.Insert(0, new Tuple<string, string, bool>(string.Empty, $"*********** OPERATION COMPLETED, Elasped time: {sw.Elapsed}", false));
                }
                finally
                {
                    BtnIndexWebText = Resources.T058;
                    IsWorking = false;
                }

                AddToScanHistory(selectedUrl);
            }
            else
            {
                BtnIndexWebText = Resources.T059;
                langAdapter.CancelOperation();
            }
        }

        private void OnProgressChanged(object htmlScanResult)
        {
            HtmlScanResult htmlScan = htmlScanResult as HtmlScanResult;

            App.Current.Dispatcher.Invoke(() =>
            {
                if (htmlScan.IsError)
                {
                    ErrorURLCount++;
                    ScanOutput.Insert(0, new Tuple<string, string, bool>(string.Empty, htmlScan.ErrorMessage, true));
                }
                else
                {
                    SuccessURLCount++;
                    ScanOutput.Insert(0, new Tuple<string, string, bool>((SuccessURLCount).ToString(), htmlScan.URL.ToString(), false));

                    if (SuccessURLCount >= maxLimit)
                        langAdapter?.CancelOperation();
                }
            });
        }
    }
}
