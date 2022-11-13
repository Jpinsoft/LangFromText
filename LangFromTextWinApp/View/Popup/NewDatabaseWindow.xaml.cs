using Jpinsoft.LangTainer.CBO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LangFromTextWinApp.View.Popup
{
    /// <summary>
    /// Interaction logic for NewDatabaseWindow.xaml
    /// </summary>
    public partial class NewDatabaseWindow : Window
    {
        public LangDatabaseInfo NewLangDatabaseInfo { get; set; }

        public NewDatabaseWindow()
        {
            InitializeComponent();
        }

        public NewDatabaseWindow(Window owner, string selectedFolder)
        {
            InitializeComponent();

            this.Owner = owner;
            TxbDBFolder.Text = selectedFolder;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxbDatabaseName.Text))
            {
                TxbDatabaseName.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }

            this.DialogResult = true;
            this.NewLangDatabaseInfo = new LangDatabaseInfo
            {
                DataSource = TxbDBFolder.Text,
                DBName = TxbDatabaseName.Text
            };

            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void BtnSetDataFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbDialog = new FolderBrowserDialog { SelectedPath = TxbDBFolder.Text };

            if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TxbDBFolder.Text = fbDialog.SelectedPath;
            }
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                BtnCancel_Click(this, null);
            }

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                BtnOk_Click(this, null);
            }
        }
    }
}
