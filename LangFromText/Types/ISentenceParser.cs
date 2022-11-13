using Jpinsoft.LangTainer.CBO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jpinsoft.LangTainer.Types
{
    public interface ISentenceParser
    {
        /// <summary>
        /// Metoda pozadovanou implementaciou parsera viet vykona rozklad vety/textu na jednotlive slova
        /// Vystupne slova nie su vlozene do Databazy a je potrebne ich dalej spracovat.
        /// </summary>
        List<string> ParseToWords(string sentence);
    }
}
