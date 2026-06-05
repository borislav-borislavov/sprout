using Sprout.Core.Common;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Json;

public static class DataTableJsonExtensions
{
    public static string ToJson(this DataTable table)
    {
        if (table == null || table.Rows.Count == 0) return "[]";

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            table.SerializeToWriter(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Separating the logic allows you to stream directly to web responses or files
    public static void SerializeToWriter(this DataTable table, Utf8JsonWriter writer)
    {
        writer.WriteStartArray();

        foreach (DataRow row in table.Rows)
        {
            writer.WriteStartObject();
            foreach (DataColumn col in table.Columns)
            {
                if(col.ColumnName == Const.BuiltInDataTableColumns._IsDeleted
                    || col.ColumnName == Const.BuiltInDataTableColumns._RowBackColor)
                {
                    continue;
                }

                writer.WritePropertyName(col.ColumnName);
                object value = row[col];

                if (value == null || value == DBNull.Value)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    // Pattern matching handles common types directly to bypass reflection
                    switch (value)
                    {
                        case string s: writer.WriteStringValue(s); break;
                        case int i: writer.WriteNumberValue(i); break;
                        case long l: writer.WriteNumberValue(l); break;
                        case double d: writer.WriteNumberValue(d); break;
                        case decimal dec: writer.WriteNumberValue(dec); break;
                        case bool b: writer.WriteBooleanValue(b); break;
                        case DateTime dt: writer.WriteStringValue(dt); break;
                        default: JsonSerializer.Serialize(writer, value, value.GetType()); break;
                    }
                }
            }
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}
