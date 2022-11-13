using Jpinsoft.LangTainer.Types;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LangFromTextWinApp.Controls
{
    /// <summary>
    /// Interaction logic for ProgressContentOverlayControl.xaml
    /// </summary>
    public partial class ProgressContentOverlayControl : UserControl, IProgressControl
    {
        public bool CancelSignal { get; set; }

        private ContentControl parrentContentControl;

        public object ParrentContentControl
        {
            get { return parrentContentControl; }
            set
            {
                parrentContentControl = value as ContentControl;

                if (parrentContentControl == null)
                    throw new NotSupportedException("ProgressContentOverlayControl require WPF ContentControl as parrentContentControl.");
            }
        }

        private object origContent;

        public ProgressContentOverlayControl(ContentControl parrentContentControl)
        {
            InitializeComponent();

            ParrentContentControl = parrentContentControl;
        }

        public void ShowProgress(bool allowCancel, string title = null)
        {
            Width = parrentContentControl.ActualWidth;
            Height = parrentContentControl.ActualHeight;

            LblTitle.Content = string.IsNullOrEmpty(title) ? Properties.Resources.T067 : title;

            origContent = parrentContentControl.Content;
            parrentContentControl.Content = this;
            BtnCancelProgress.Visibility = Visibility.Collapsed;

            if (allowCancel)
            {
                BtnCancelProgress.Visibility = Visibility.Visible;
            }
        }

        public void CloseProgress()
        {
            parrentContentControl.Content = origContent;
        }

        private void BtnCancelProgress_Click(object sender, RoutedEventArgs e)
        {
            CancelSignal = true;
        }
    }
}
