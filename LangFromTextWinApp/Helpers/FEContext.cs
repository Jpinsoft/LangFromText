using Jpinsoft.LangTainer;
using Jpinsoft.LangTainer.Data;
using LangFromTextWinApp.LTModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LangFromTextWinApp.Helpers
{
    public class FEContext
    {
        public static MenuNavigator MNavigator { get; set; }

        public static MainWindow MainWin { get; set; }

        public static string AppDataFolder { get; set; }

        public static LangModulesDataRepository ModulesRepository { get; set; }

        public static LangFromTextManager LangFromText;

        public static List<ILTModuleView> LTModules { get; set; } = new List<ILTModuleView>();

        public static LTTimer LangFromTextTimer { get; set; }

        public static bool IsAutorun { get; set; }
    }
}
