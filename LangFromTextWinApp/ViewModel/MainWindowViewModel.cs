using Jpinsoft.LangTainer;
using Jpinsoft.LangTainer.Data;
using Jpinsoft.LangTainer.Types;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.LTModules.EnterChar;
using LangFromTextWinApp.LTModules.SelectPhrase;
using LangFromTextWinApp.LTModules.SelectWord;
using LangFromTextWinApp.LTModules.Vocabulary;
using LangFromTextWinApp.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace LangFromTextWinApp.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {

        }

        private bool isLTModuleEnabled = true;

        public bool IsLTModuleEnabled
        {
            get { return isLTModuleEnabled; }
            set { SetProperty(ref isLTModuleEnabled, value); }
        }

        private bool isCheckPhraseEnabled = true;

        public bool IsCheckPhraseEnabled
        {
            get { return isCheckPhraseEnabled; }
            set { SetProperty(ref isCheckPhraseEnabled, value); }
        }

        public async Task Init(IProgressControl progressControl)
        {
            FEContext.AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FEConstants.PRODUCT_NAME);
            FEContext.ModulesRepository = new LangModulesDataRepository(FEContext.AppDataFolder);

            if (!Directory.Exists(FEContext.AppDataFolder))
                Directory.CreateDirectory(FEContext.AppDataFolder);

            try
            {
                progressControl.ShowProgress(false, Resources.T068);
                FEContext.MainWin.MenuMainVertical.IsEnabled = false;

                if (string.IsNullOrEmpty(Settings.Default.DataConnString))
                { // Default ConnString
                    FEContext.LangFromText = await CreateDefaultDatabase();
                }
                else
                {
                    FEContext.LangFromText = new LangFromTextManager(Settings.Default.DataConnString);
                    await FEContext.LangFromText.Init();
                }
            }
            catch (Exception ex)
            {
                MessageBoxWPF.ShowWarning(FEContext.MainWin, MessageBoxButton.OK, Properties.Resources.T022 + ex.Message);
                FEContext.LangFromText = await CreateDefaultDatabase();
            }
            finally
            {
                progressControl.CloseProgress();
                FEContext.MainWin.MenuMainVertical.IsEnabled = true;
            }

            FEContext.MainWin.RefreshState();

            // TODO START TIMER
            FEContext.LTModules = new List<LTModules.ILTModuleView> { new SelectWordModule(), new EnterCharModule(), new SelectPhraseModule(), new VocabularyModule() };

            FEContext.MNavigator.ShowStartPage();
            FEContext.LangFromTextTimer = new LTTimer();
            FEContext.LangFromTextTimer.SetLTActivationTimer(Settings.Default.ActivateLTModuleIntervalMinutes);
        }

        public void RefreshMenuState()
        {
            IsCheckPhraseEnabled = FEContext.LangFromText.HasEnoughtWords;
            IsLTModuleEnabled = FEContext.LangFromText.HasEnoughtWords;
        }

        private async Task<LangFromTextManager> CreateDefaultDatabase()
        {
            string defaultDBName = $"{DateTime.Now.ToString("yyyy-MM-dd")}-{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}-LangFromTextDefault";
            string defaultFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FEConstants.PRODUCT_NAME);

            try
            {
                if (!Directory.Exists(defaultFolder))
                    Directory.CreateDirectory(defaultFolder);
            }
            catch
            {
                defaultFolder = FEContext.AppDataFolder;

                if (!Directory.Exists(defaultFolder))
                    Directory.CreateDirectory(defaultFolder);
            }

            using (File.Create(Path.Combine(defaultFolder, defaultDBName + ".ltdb"))) { }
            Settings.Default.DataConnString = string.Format("DataSource={0}; DBName={1}", defaultFolder, defaultDBName);
            Settings.Default.Save();

            LangFromTextManager newLangFromTextManager = new LangFromTextManager(Settings.Default.DataConnString);
            await newLangFromTextManager.Init();

            return newLangFromTextManager;
        }

        public void Dispose()
        {
            try
            {
                Settings.Default.Save();
                FEContext.LangFromTextTimer?.Stop();

                FEContext.ModulesRepository.Repository.ForEach(mStorage => mStorage.Save());
            } // TODO Event log
            catch { }
        }
    }
}
