using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.UIStates;
using Sprout.Core.Views;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Sprout.Core.Factories
{
    public class SproutDataGridFactory : BaseSproutControlFactory
    {
        internal static SproutDataGrid GenerateSproutGrid(SproutDataGridConfig sproutGridConfig, Dictionary<string, UIElement> controls)
        {
            var sproutDataGrid = new SproutDataGrid
            {
                Name = sproutGridConfig.Name,
                Config = sproutGridConfig,
            };

            foreach (var colConfig in sproutGridConfig.Columns ?? [])
            {
                DataGridColumn col = null;

                if (colConfig.ColumnType == ColumnType.Check)
                {
                    col = new DataGridCheckBoxColumn()
                    {
                        Header = colConfig.Header,
                        Binding = new System.Windows.Data.Binding(colConfig.BindingPath),
                        Width = DataGridLength.Auto,
                        IsReadOnly = colConfig.IsReadOnly
                    };
                }
                else
                {
                    col = new DataGridTextColumn()
                    {
                        Header = colConfig.Header,
                        Binding = new System.Windows.Data.Binding(colConfig.BindingPath),
                        Width = DataGridLength.Auto,
                        IsReadOnly = colConfig.IsReadOnly
                    };
                }

                sproutDataGrid.dataGrid.Columns.Add(col);

            }

            AddControl(sproutDataGrid, controls);

            SetGridPosition(sproutDataGrid, sproutGridConfig);

            var gridUIState = new SproutGridUIState();
            gridUIState.SetUpState(sproutDataGrid);

            return sproutDataGrid;
        }
    }
}
