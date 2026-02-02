using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations;
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
        [ObservableProperty]
        private SqlServerFilterConfig _selectedFilter;

        public IEnumerable<EditorType> EditorTypes => Enum.GetValues(typeof(EditorType)).Cast<EditorType>();

        [RelayCommand]
        private void AddFilter()
        {
            if (SelectedNode == null) return;

            if (SelectedNode is not SproutDataGridConfig dataGrid) return;

            if (dataGrid.DataAdapter is not SqlServerDataAdapterConfig adapterConfig) return;

            if (adapterConfig.DataProvider is not SqlServerDataProviderConfig dataProvider) return;

            dataProvider.FilterConfigs.Add(new SqlServerFilterConfig
            {
                Title = "New Filter",
                Text = "",
                EditorType = EditorType.TextBox
            });
        }


        [RelayCommand]
        private void RemoveFilter()
        {
            if (SelectedFilter == null) return;

            if (SelectedNode == null) return;

            if (SelectedNode is not SproutDataGridConfig dataGrid) return;

            if (dataGrid.DataAdapter is not SqlServerDataAdapterConfig adapterConfig) return;

            if (adapterConfig.DataProvider is not SqlServerDataProviderConfig dataProvider) return;

            dataProvider.FilterConfigs.Remove(SelectedFilter);
        }
    }
}
