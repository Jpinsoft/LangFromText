using System;
using System.Collections.Generic;
using System.Text;

namespace Jpinsoft.LangTainer.Utils
{
    public class ExceptionUtils
    {
        /// <summary>
        /// Rekurzivna metoda vrati vsetky spravy z vnorenych vynimiek nachadzajucich sa v ex
        /// </summary>
        public static string AddInnerMessages(Exception ex)
        {
            if (ex.InnerException == null)
                return ex.Message;

            string message;

            message = ex.Message + " - Inner exception : " + AddInnerMessages(ex.InnerException);

            return message;
        }
    }
}
