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
    /// <summary>
    /// Pocita s MAX VALUE ako INT <= 100
    /// </summary>
    public class RatingToBrushConverter : IValueConverter
    {
        private const int strmost = 4; // Max 6 inak pretecie BYTE
        private const int colorBase = 0x40;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double rating = (double)value;

                Color c = Color.FromRgb((byte)(colorBase + rating * strmost), 0x0A, 0x0A);

                if (rating >= 30 && rating <= 70)
                    c = Color.FromRgb(0x0A, 0x0A, (byte)(colorBase + (rating - 30) * strmost));

                if (rating > 70)
                    c = Color.FromRgb(0x0A, (byte)(colorBase + (rating - 70) * strmost), 0x0A);

                return new SolidColorBrush(c);
            }

            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
