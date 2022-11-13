using Jpinsoft.LangTainer.CBO;
using LangFromTextWinApp.ViewModel.Popup;
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

namespace LangFromTextWinApp.View.Popup
{
    /// <summary>
    /// Interaction logic for TextSourceDetailUC.xaml
    /// </summary>
    public partial class TextSourceDetailUC : UserControl
    {
        public TextSourceDetailViewModel ViewModel { get { return this.DataContext as TextSourceDetailViewModel; } }

        public TextSourceDetailUC()
        {
            InitializeComponent();
        }

        public void Init(TextSourceCBO textSource, Action onHide)
        {
            this.DataContext = new TextSourceDetailViewModel(textSource) { OnHideAction = onHide };
        }

        private void BtnHide_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Hide();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.ToString()));
            e.Handled = true;
        }
    }
}
