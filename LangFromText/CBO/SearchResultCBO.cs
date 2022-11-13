using System;
using System.Collections.Generic;
using System.Text;

namespace Jpinsoft.LangTainer.CBO
{
    public class SearchResultCBO
    {
        public PhrasePattern PPattern { get; set; }

        public PhraseCBO FoundedPhrase { get; set; }

        public PhraseCBO Sentence { get; set; }

        public int Index { get; set; }

        public double Rating { get; set; }
    }
}
