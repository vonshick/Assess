using System;
using System.Globalization;
using System.Windows.Data;

namespace UTA.Converters
{
    internal class IndexToRankConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int index ? index + 1 : (object) null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int index ? index - 1 : (object) null;
        }
    }
}