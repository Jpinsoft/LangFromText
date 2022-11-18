using System;
using System.Collections.Generic;
using System.Text;

namespace Jpinsoft.LangTainer.Types
{
    public interface IProgressControl
    {
        object ParrentContentControl { get; set; }

        bool CancelSignal { get; set; }

        void ShowProgress(bool allowCancel, string title = null);

        string Title { get; set; }

        void CloseProgress();
    }
}
