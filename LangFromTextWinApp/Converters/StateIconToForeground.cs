using FontAwesome.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace LangFromTextWinApp.Converters
{
    public class StateIconToForeground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((FontAwesomeIcon)value)
            {
                case FontAwesomeIcon.Warning:
                    return Brushes.DarkOrange;

                case FontAwesomeIcon.CheckCircle:
                case FontAwesomeIcon.ClockOutline:
                case FontAwesomeIcon.PlayCircle:
                    return Brushes.Green;

                case FontAwesomeIcon.StopCircle:
                case FontAwesomeIcon.PowerOff:
                    return Brushes.DarkRed;
            }

            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
