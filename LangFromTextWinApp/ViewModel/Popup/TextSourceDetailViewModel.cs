using Jpinsoft.LangTainer.CBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangFromTextWinApp.ViewModel.Popup
{
    public class TextSourceDetailViewModel
    {
        public Action OnHideAction { get; set; }

        public TextSourceCBO TextSource { get; set; }

        public IEnumerable<PhraseCBO> SentencesDistinct { get; set; }

        public TextSourceDetailViewModel(TextSourceCBO tSource)
        {
            TextSource = tSource;
            SentencesDistinct = tSource.Sentences.Distinct();
        }

        public void Hide()
        {
            OnHideAction?.Invoke();
        }
    }
}
