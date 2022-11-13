using Jpinsoft.LangTainer.CBO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jpinsoft.LangTainer.Types
{
    public interface ILangAdapter
    {
        object AdapterSettings { get; set; }

        List<TextSourceCBO> GetTextSources(string sourceAddress, Action<object> onProgressChanged = null);

        void CancelOperation();
    }
}
