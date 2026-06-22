using System;
using System.Globalization;
using System.Windows.Data;

namespace Sprout.Core.Converters
{
    /// <summary>
    /// Converts between double? and string so that an empty TextBox clears the value to null.
    /// </summary>
    public class NullableDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;

            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                return result;

            return null;
        }
    }
}
