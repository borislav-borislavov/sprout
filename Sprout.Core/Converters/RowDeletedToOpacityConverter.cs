using Sprout.Core.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Sprout.Core.Converters
{
    public class RowDeletedToOpacityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] is the whole Row (DataRowView) - helps with Recycling
            // values[1] is the specific value [_IsDeleted] - helps with Instant Updates

            if (values[0] is DataRowView drv)
            {
                if (drv.Row[Const.BuiltInDataTableColumns._IsDeleted] is bool isDeleted && isDeleted) //this triggers instantly when the value is flagged but before the row state is set
                {
                    return 0.3;
                }
            }

            return 1.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
