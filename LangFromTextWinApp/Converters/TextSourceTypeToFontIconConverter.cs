using FontAwesome.WPF;
using Jpinsoft.LangTainer.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LangFromTextWinApp.Converters
{
    public class TextSourceTypeToFontIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((TextSourceTypeEnum)value)
            {
                case TextSourceTypeEnum.File:
                    return FontAwesomeIcon.FileText;

                case TextSourceTypeEnum.URL_HTML:
                    return FontAwesomeIcon.InternetExplorer;

                default:
                    return FontAwesomeIcon.None;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
