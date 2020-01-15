using System;
using System.Globalization;
using System.Windows.Data;

namespace UTA.Converters
{
    public class AssessMethodConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var integer = (int) value;
                if (parameter != null && integer == int.Parse(parameter.ToString()))
                    return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}