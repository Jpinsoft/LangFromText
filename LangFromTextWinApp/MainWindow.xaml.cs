using LangFromTextWinApp.Controls;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.LTModules.EnterChar;
using LangFromTextWinApp.LTModules.SelectPhrase;
using LangFromTextWinApp.LTModules.SelectWord;
using LangFromTextWinApp.LTModules.BuildSentence;
using LangFromTextWinApp.Properties;
using LangFromTextWinApp.ViewModel;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using MenuItem = System.Windows.Controls.MenuItem;

namespace LangFromTextWinApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public NotifyIcon notifIcon = new NotifyIcon();
        public MainWindowViewModel ViewModel { get { return this.DataContext as MainWindowViewModel; } }
        private NamedPipeTools pipesHelper = new NamedPipeTools();

        public MainWindow()
        {
            InitializeComponent();
            InitNotifyIcon();

            FEContext.MNavigator = new MenuNavigator(MenuMainVertical, MainContent);
        }

        private void PipesHelper_DateReceived(int command)
        {
            if (command == NamedPipeTools.COMM_SHOWWINDOW)
            {
                Dispatcher.Invoke(() =>
                {
                    ShowOnTop();
                });
            }
        }

        public void ShowOnTop()
        {
            this.Show();

            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;

            this.Activate();
            this.Topmost = true;  // important
            this.Topmost = false; // important
            this.Focus();
        }

        #region NotifyIcon

        private void InitNotifyIcon()
        {
            notifIcon.Icon = new System.Drawing.Icon("MainLT.ico", 16, 16);
            notifIcon.Visible = true;

            notifIcon.ContextMenu = new ContextMenu(new System.Windows.Forms.MenuItem[]
            {
                new System.Windows.Forms.MenuItem(Properties.Resources.T085, NotifyContextMenuOpen_Click),
                new System.Windows.Forms.MenuItem(Properties.Resources.T086, NotifyContextMenuShowRandomQuestion_Click),
                new System.Windows.Forms.MenuItem("-"),
                new System.Windows.Forms.MenuItem(Properties.Resources.T087, NotifyContextMenuClose_Click)
            });

            notifIcon.DoubleClick += NotifyContextMenuOpen_Click;
        }

        public void NotifyContextMenuOpen_Click(object sender, EventArgs e)
        {
            ShowOnTop();
        }

        public void NotifyContextMenuShowRandomQuestion_Click(object sender, EventArgs e)
        {
            ShowOnTop();

            FEContext.MNavigator.ShowRandomLangModule();
        }

        public void NotifyContextMenuClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        public void RefreshState()
        {
            ViewModel.RefreshMenuState();
        }

        #region Events

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new MainWindowViewModel();
            this.Title = $"{FEConstants.PRODUCT_NAME} {Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}";
            FEContext.MainWin = this;

            this.Width = Settings.Default.WinSizeWidth;
            this.Height = Settings.Default.WinSizeHeight;

            if (FEContext.IsAutorun)
                this.WindowState = WindowState.Minimized;

            // Dolezite zachovat taketo poradie - Init moze trvat dlho, preto Minimize a set viewmodel je este predtym
            try
            {
                await ViewModel.Init(new ProgressContentOverlayControl(MainContent));
            }
            catch (Exception ex)
            {
                if (!FEContext.IsAutorun)
                    MessageBoxWPF.ShowError(FEContext.MainWin, MessageBoxButton.OK, Properties.Resources.T079 + ex.Message);

                // TODO else EventLog

                this.Close();
                return;
            }

            pipesHelper.StartNamedPipeServer(PipesHelper_DateReceived);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);

            WPFHelpers.PlaceWindowToScreen(this);
        }

        private void MenuItemStandard_Click(object sender, RoutedEventArgs e)
        {
            FEContext.MNavigator.ShowPageByMenuTag(sender as MenuItem);
        }

        private void SelectWordModule_Click(object sender, RoutedEventArgs e)
        {
            FEContext.MNavigator.MarkSelectedMenuItem(MenuItemLangTraining);

            FEContext.MNavigator.ShowControl(FEContext.LTModules.First(j => j is SelectWordModule));
        }

        private void EnterCharModule_Click(object sender, RoutedEventArgs e)
        {
            FEContext.MNavigator.MarkSelectedMenuItem(MenuItemLangTraining);

            FEContext.MNavigator.ShowControl(FEContext.LTModules.First(j => j is EnterCharModule));
        }

        private void SelectPhraseModule_Click(object sender, RoutedEventArgs e)
        {
            FEContext.MNavigator.MarkSelectedMenuItem(MenuItemLangTraining);

            FEContext.MNavigator.ShowControl(FEContext.LTModules.First(j => j is SelectPhraseModule));
        }

        private void TranslateWordModule_Click(object sender, RoutedEventArgs e)
        {
            FEContext.MNavigator.MarkSelectedMenuItem(MenuItemLangTraining);

            FEContext.MNavigator.ShowControl(FEContext.LTModules.First(j => j is LTModules.Vocabulary.VocabularyModule));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                Settings.Default.WinSizeWidth = this.Width;
                Settings.Default.WinSizeHeight = this.Height;
            }

            Settings.Default.Save();
            notifIcon?.Dispose();
            ViewModel.Dispose();
        }

        #endregion

        private void BuildSentenceModule_Click(object sender, RoutedEventArgs e)
        {
            FEContext.MNavigator.MarkSelectedMenuItem(MenuItemLangTraining);

            FEContext.MNavigator.ShowControl(FEContext.LTModules.First(j => j is BuildSentenceModule));
        }
    }
}