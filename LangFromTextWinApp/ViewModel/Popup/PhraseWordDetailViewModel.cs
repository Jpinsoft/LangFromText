using Jpinsoft.LangTainer.CBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangFromTextWinApp.ViewModel.Popup
{
    public class PhraseWordDetailViewModel
    {
        public Action OnHideAction { get; set; }

        public SearchResultsViewModel SearchResults { get; set; }

        public List<TextSourceCBO> TextSources { get; set; }

        public void Hide()
        {
            OnHideAction?.Invoke();
        }

    }
}
