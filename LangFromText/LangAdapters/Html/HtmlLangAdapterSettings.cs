using System;
using System.Collections.Generic;
using System.Text;

namespace Jpinsoft.LangTainer.LangAdapters.Html
{
    public class HtmlLangAdapterSettings
    {
        public int HtmlScanLevel { get; set; }

        public string AllowedURLContains { get; set; }

        public string AllowedURLNotContains { get; set; }
    }
}
