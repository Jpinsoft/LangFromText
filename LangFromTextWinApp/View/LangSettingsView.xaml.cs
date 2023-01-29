using LangFromTextWinApp.Controls;
using LangFromTextWinApp.ViewModel;
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

namespace LangFromTextWinApp.View
{
    /// <summary>
    /// Interaction logic for LangSettingsView.xaml
    /// </summary>
    public partial class LangSettingsView : UserControl
    {
        public LangSettingsViewModel ViewModel { get { return DataContext as LangSettingsViewModel; } }

        public LangSettingsView()
        {
            InitializeComponent();

            this.DataContext = new LangSettingsViewModel();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Init();
        }

        private void BtnNewDatabase_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CreateDatabase();
        }

        private void BtnLoadDatabase_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadDatabase(new ProgressContentOverlayControl(this));
        }

        private void BtnTranslatorLinkReset_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TranslatorLinkReset();
        }
    }
}
