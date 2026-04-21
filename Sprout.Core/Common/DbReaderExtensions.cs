using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Common
{
    public static class DbReaderExtensions
    {
        public static string? GetStringOrNull(this IDataReader reader, int i)
        {
            if (reader.IsDBNull(i)) return null;

            return reader.GetString(i);
        }

        public static int? GetIntOrNull(this IDataReader reader, int i)
        {
            if (reader.IsDBNull(i)) return null;

            return reader.GetInt32(i);
        }

        public static bool? GetBoolOrNull(this IDataReader reader, int i)
        {
            if (reader.IsDBNull(i)) return null;

            return reader.GetBoolean(i);
        }
    }
}
