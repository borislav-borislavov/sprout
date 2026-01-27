using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations.DataGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.ViewModels
{
    public partial class EditPageVM : ObservableObject
    {
        private SproutDataGridConfig SelectedDataGrid => this.SelectedNode as SproutDataGridConfig;

        [ObservableProperty]
        private SproutDataGridColumnConfig _selectedColumn;

        [RelayCommand]
        private void AddColumn()
        {
            if (SelectedDataGrid == null) return;

            SelectedDataGrid.Columns.Add(new SproutDataGridColumnConfig());
        }

        [RelayCommand]
        private void DeleteColumn()
        {
            if (SelectedColumn != null)
            {
                SelectedDataGrid.Columns.Remove(SelectedColumn);
            }
        }
    }
}
