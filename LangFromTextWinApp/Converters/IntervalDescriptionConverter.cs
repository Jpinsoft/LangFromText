using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LangFromTextWinApp.Converters
{
    public class IntervalDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int valueInt = System.Convert.ToInt32(value);

            if (valueInt == 0)
            {
                return string.Format(Properties.Resources.T081, valueInt);
            }

            return string.Format(Properties.Resources.T082, valueInt);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
