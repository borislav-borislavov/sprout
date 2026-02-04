using Sprout.Core.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Sprout.Core.Converters
{
    public class RowColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] is the row object (DataRowView)
            // values[1] is the specific [_RowBackColor] value

            if (values[0] is DataRowView rowView 
                && rowView[Const.BuiltInDataTableColumns._RowBackColor] is string rowColor)
            {
                string colorString = $"{values[1]}";

                if (!string.IsNullOrWhiteSpace(colorString))
                {
                    try
                    {
                        // Converts hex codes (#FF0000) or named colors (Red)
                        return (SolidColorBrush)new BrushConverter().ConvertFromString(colorString);
                    }
                    catch { /* Fallback if the string is an invalid color */ }
                }
            }

            //make sure to return the default color because of recycled views
            //as an optimization one could also pass the current color and if the color is good (does not need to change) to return Binding.DoNothing;
            return Brushes.White;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
