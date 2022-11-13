using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangFromTextWinApp.LTModules
{
    public interface ILTModuleView
    {
        string ModuleName { get; }

        void ShowModule();
    }
}
