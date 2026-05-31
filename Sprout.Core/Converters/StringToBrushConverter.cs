using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Sprout.Core.Converters
{
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colorString = value as string;

            if (!string.IsNullOrWhiteSpace(colorString))
            {
                try
                {
                    // Converts hex codes (#FF0000) or named colors (Red)
                    return (SolidColorBrush)new BrushConverter().ConvertFromString(colorString);
                }
                catch { /* Fallback if the string is an invalid color */ }
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color.ToString();
            }

            return null;
        }
    }
}
