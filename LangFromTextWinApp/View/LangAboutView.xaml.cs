using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for LangAboutView.xaml
    /// </summary>
    public partial class LangAboutView : UserControl
    {
        public LangAboutViewModel ViewModel { get { return this.DataContext as LangAboutViewModel; } }

        public LangAboutView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new LangAboutViewModel();
        }

        private void BtnAppHomaPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(FEConstants.PRODUCT_URL);
        }
    }
}
