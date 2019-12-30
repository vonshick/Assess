using System;
using System.Globalization;
using System.Windows.Data;

namespace UTA.Converters
{
    internal class IndexToRankConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int) value + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int) value - 1;
        }
    }
}