using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Sprout.Core.Converters
{
    /// <summary>
    /// Converts common icon escape notations typed by the user into the actual Unicode character.
    /// Supported input formats: &#xE74D;  \uE74E  0xE74E  E74E (plain hex)
    /// </summary>
    public class IconStringConverter : IValueConverter
    {
        // Model → TextBox: pass the stored value through unchanged
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? string.Empty;
        }

        // TextBox → Model: parse escape notations into the actual character
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string input || string.IsNullOrEmpty(input))
                return value;

            var trimmed = input.Trim();

            // &#xE74D;  — XML/XAML hex entity
            var xmlHex = Regex.Match(trimmed, @"^&#x([0-9A-Fa-f]+);$");
            if (xmlHex.Success)
                return ParseCodePoint(xmlHex.Groups[1].Value) ?? value;

            // \uE74E  — C# unicode escape
            var csharpUnicode = Regex.Match(trimmed, @"^\\u([0-9A-Fa-f]{4})$");
            if (csharpUnicode.Success)
                return ParseCodePoint(csharpUnicode.Groups[1].Value) ?? value;

            // 0xE74E  — hex with 0x prefix
            var hexPrefix = Regex.Match(trimmed, @"^0[xX]([0-9A-Fa-f]+)$");
            if (hexPrefix.Success)
                return ParseCodePoint(hexPrefix.Groups[1].Value) ?? value;

            // E74E  — plain 4-5 digit hex (no prefix)
            var plainHex = Regex.Match(trimmed, @"^[0-9A-Fa-f]{4,5}$");
            if (plainHex.Success)
                return ParseCodePoint(trimmed) ?? value;

            return value;
        }

        private static string ParseCodePoint(string hex)
        {
            try
            {
                int codePoint = System.Convert.ToInt32(hex, 16);
                return char.ConvertFromUtf32(codePoint);
            }
            catch
            {
                return null;
            }
        }
    }
}
