using LangFromTextWinApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LangFromTextWinApp.ViewModel
{
    public class LangAboutViewModel
    {
        public string ProductURL
        {
            get { return $"Visit {FEConstants.PRODUCT_URL}"; }
        }

        public string ProductName
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return (attributes?.Length > 0) ? ((AssemblyProductAttribute)attributes[0]).Product : string.Empty;
            }
        }

        public string Version { get { return String.Format("Version {0}, November 2024", Assembly.GetExecutingAssembly().GetName().Version); } }

        public string Copyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                return (attributes?.Length > 0) ? ((AssemblyCopyrightAttribute)attributes[0]).Copyright : string.Empty;
            }
        }
    }
}
