using Sprout.Core.Common;
using Sprout.Core.Factories;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.SproutControlVMs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Features.ButtonActions.GridActions
{
    public class AddRowGridAction : IButtonAction
    {
        private readonly string _ownControlName;

        public AddRowGridAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public Task Perform(VMRegistry vmRegistry, IDataServiceFactory dataServiceFactory)
        {
            var ownDataAdapter = vmRegistry.GetAdapterOrThrow(_ownControlName);

            var newRow = ownDataAdapter.DataProvider.Data.NewRow();

            ApplyDefaultValues(vmRegistry, newRow);

            ownDataAdapter.DataProvider.Data.Rows.Add(newRow);

            return Task.CompletedTask;
        }

        private void ApplyDefaultValues(VMRegistry vmRegistry, DataRow newRow)
        {
            if (vmRegistry[_ownControlName] is not SproutDataGridVM gridVM)
                return;

            var columns = gridVM.Grid?.Config?.Columns;
            if (columns == null)
                return;

            foreach (var colConfig in columns.Where(c => !string.IsNullOrEmpty(c.DefaultValue)))
            {
                var columnName = colConfig.BindingPath;

                if (string.IsNullOrEmpty(columnName) || !newRow.Table.Columns.Contains(columnName))
                    throw new Exception($"Cannot apply default value for column '{colConfig.Header ?? columnName}': column '{columnName}' does not exist in the data table of '{_ownControlName}'.");

                var dataColumn = newRow.Table.Columns[columnName];
                newRow[dataColumn] = ConvertDefaultValue(colConfig.DefaultValue, dataColumn);
            }
        }

        private static object ConvertDefaultValue(string defaultValue, DataColumn dataColumn)
        {
            try
            {
                var targetType = dataColumn.DataType;

                if (targetType == typeof(string))
                    return defaultValue;

                if (targetType == typeof(bool))
                    return bool.Parse(defaultValue);

                if (targetType == typeof(DateTime))
                    return DateTime.Parse(defaultValue, CultureInfo.InvariantCulture);

                if (targetType == typeof(Guid))
                    return Guid.Parse(defaultValue);

                return Convert.ChangeType(defaultValue, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot convert default value '{defaultValue}' to type '{dataColumn.DataType.Name}' for column '{dataColumn.ColumnName}'.", ex);
            }
        }
    }
}
