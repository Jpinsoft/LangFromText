using Jpinsoft.LangTainer;
using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.Data;
using Jpinsoft.LangTainer.Types;
using LangFromTextWinApp.Controls;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.LTModules.EnterChar;
using LangFromTextWinApp.LTModules.SelectPhrase;
using LangFromTextWinApp.LTModules.SelectWord;
using LangFromTextWinApp.Properties;
using LangFromTextWinApp.View.Popup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LangFromTextWinApp.ViewModel
{
    public class LangSettingsViewModel : ViewModelBase
    {
        private bool autoStart;

        public bool AutoStart
        {
            get { return autoStart; }
            set
            {
                SetProperty(ref autoStart, value);
                SetAutoStart(value);
            }
        }

        private string dataFolder;

        public string DataFolder
        {
            get { return dataFolder; }
            set { SetProperty(ref dataFolder, value); }
        }

        private string selectedTheme;

        public string SelectedTheme
        {
            get { return selectedTheme; }
            set
            {
                SetProperty(ref selectedTheme, value);
                ChangeTheme(value);
            }
        }

        public List<Tuple<string, string>> Languages { get; set; }

        private Tuple<string, string> selectedLanguage;

        public Tuple<string, string> SelectedLanguage
        {
            get { return selectedLanguage; }
            set
            {
                SetProperty(ref selectedLanguage, value);
                Settings.Default.AppLanguage = value.Item2;
            }
        }

        private LangDatabaseInfo selectedDatabase;

        public LangDatabaseInfo SelectedDatabase
        {
            get { return selectedDatabase; }
            set { SetProperty(ref selectedDatabase, value); }
        }

        OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "LT Database files (*.ltdb)|*.ltdb" };

        private int activationInterval;

        public int ActivationInterval
        {
            get { return activationInterval; }
            set
            {
                SetProperty(ref activationInterval, value);
                Settings.Default.ActivateLTModuleIntervalMinutes = value;
                FEContext.LangFromTextTimer.SetLTActivationTimer(Settings.Default.ActivateLTModuleIntervalMinutes);
            }
        }

        private bool isTimerSettingEnabled;

        public bool IsTimerSettingEnabled
        {
            get { return isTimerSettingEnabled; }
            set { SetProperty(ref isTimerSettingEnabled, value); }
        }

        private string translatorLink;

        public string TranslatorLink
        {
            get { return translatorLink; }
            set
            {
                SetProperty(ref translatorLink, value);

                Settings.Default.URLTemplateOpenTranslator = value;
            }
        }

        public LangSettingsViewModel()
        {
            Languages = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(Resources.T061, string.Empty),
                new Tuple<string, string>(Resources.T062, "sk-SK"),
                new Tuple<string, string>(Resources.T063, "en-US")
            };
        }

        public void Init()
        {
            LangDatabaseInfo ltDbInfo = new LangDatabaseInfo(Settings.Default.DataConnString);
            ltDbInfo.Size = new FileInfo(Path.Combine(ltDbInfo.DataSource, ltDbInfo.DBName + ".ltdb")).Length;
            SelectedDatabase = ltDbInfo;
            ActivationInterval = FEContext.LangFromText.HasEnoughtWords ? Settings.Default.ActivateLTModuleIntervalMinutes : 0;
            IsTimerSettingEnabled = FEContext.LangFromText.HasEnoughtWords;

            SelectedTheme = Settings.Default.CurrentTheme;
            SelectedLanguage = Languages.FirstOrDefault(l => l.Item2 == Settings.Default.AppLanguage);
            AutoStart = Settings.Default.AutoStart;
            TranslatorLink = Settings.Default.URLTemplateOpenTranslator;
        }

        public async void LoadDatabase(IProgressControl progressControl)
        {
            openFileDialog.InitialDirectory = SelectedDatabase.DataSource;
            openFileDialog.FileName = string.Empty;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LangDatabaseInfo newDbInfo = new LangDatabaseInfo
                {
                    DataSource = Path.GetDirectoryName(openFileDialog.FileName),
                    DBName = Path.GetFileNameWithoutExtension(openFileDialog.FileName),
                    Size = new FileInfo(openFileDialog.FileName).Length
                };

                try
                {
                    progressControl.ShowProgress(false, Resources.T068);
                    FEContext.MainWin.MenuMainVertical.IsEnabled = false;

                    await ChangeCurrentDatabase(newDbInfo);
                }
                finally
                {
                    progressControl.CloseProgress();
                    FEContext.MainWin.MenuMainVertical.IsEnabled = true;
                }
            }
        }

        public async void CreateDatabase()
        {
            NewDatabaseWindow newDatabaseWindow = new NewDatabaseWindow(FEContext.MainWin, SelectedDatabase.DataSource);

            if (newDatabaseWindow.ShowDialog() == true)
            {
                using (File.Create(Path.Combine(newDatabaseWindow.NewLangDatabaseInfo.DataSource, newDatabaseWindow.NewLangDatabaseInfo.DBName + ".ltdb"))) { }
                await ChangeCurrentDatabase(newDatabaseWindow.NewLangDatabaseInfo);
            }
        }

        private async Task ChangeCurrentDatabase(LangDatabaseInfo newDatabaseInfo)
        {
            LangFromTextManager newLangFromTextManager = new LangFromTextManager(newDatabaseInfo.ConnString);
            await newLangFromTextManager.Init();
            SelectedDatabase = newDatabaseInfo;

            FEContext.LangFromText = newLangFromTextManager;
            Settings.Default.DataConnString = newDatabaseInfo.ConnString;

            FEContext.MainWin.RefreshState();

            IsTimerSettingEnabled = FEContext.LangFromText.HasEnoughtWords;

            if (!FEContext.LangFromText.HasEnoughtWords)
                ActivationInterval = 0;
        }

        public void ChangeTheme(string theme)
        {
            if (theme != Settings.Default.CurrentTheme)
            {
                WPFHelpers.SetTheme(theme);

                Settings.Default.CurrentTheme = theme;
            }
        }

        public void SetAutoStart(bool autoStart)
        {
            if (autoStart != Settings.Default.AutoStart)
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();

                if (autoStart)
                    key.SetValue(curAssembly.GetName().Name, $"\"{curAssembly.Location}\" -autorun");
                else
                    key.DeleteValue(curAssembly.GetName().Name, false);

                Settings.Default.AutoStart = AutoStart;
            }
        }

        public void TranslatorLinkReset()
        {
            TranslatorLink = "https://translate.google.com/?sl=auto&tl=en&text=" + FEConstants.PLACEHOLDER_WORD;
        }
    }
}
