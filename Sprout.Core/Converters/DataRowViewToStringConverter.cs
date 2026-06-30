using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Sprout.Core.Converters
{
    /// <summary>
    /// Produces a readable summary of a <see cref="DataRowView"/> by joining its column values.
    /// Used as the default/fallback item template for the <see cref="Views.Controls.SproutList"/>
    /// so a list shows real data even before a custom item template is configured.
    /// </summary>
    public class DataRowViewToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DataRowView rowView)
            {
                return value?.ToString() ?? string.Empty;
            }

            var sb = new StringBuilder();

            foreach (DataColumn column in rowView.Row.Table.Columns)
            {
                if (column.ColumnName.StartsWith("_"))
                {
                    // Skip internal/built-in columns (e.g. row state helpers).
                    continue;
                }

                var cellValue = rowView[column.ColumnName];

                if (cellValue == null || cellValue == DBNull.Value)
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append("   |   ");
                }

                sb.Append(column.ColumnName).Append(": ").Append(cellValue);
            }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
