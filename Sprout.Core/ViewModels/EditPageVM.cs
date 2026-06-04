using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Common;
using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.Configurations.Duck;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Services.Navigation;
using Sprout.Core.Views;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

namespace Sprout.Core.ViewModels
{
    public partial class EditPageVM : ObservableObject
    {
        public SproutPageConfiguration PageConfig { get; set; }

        [ObservableProperty]
        private IDataAdapterConfig _selectedDataAdapter;

        [ObservableProperty]
        private GridConfig _selectedGridConfig;

        [ObservableProperty]
        private ObservableCollection<SproutControlConfig> _controls = [];

        [ObservableProperty]
        private SproutControlConfig _selectedNode;

        private readonly IConfigurationService _configService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        public string[] AdapterTypes { get; set; } = ["SqlServer", "Duck"];

        [ObservableProperty]
        private string _selectedAdapterType;

        [ObservableProperty]
        private ObservableObject _selectedAdapterViewModel;

        [ObservableProperty]
        private bool _areFiltersVisible;

        [ObservableProperty]
        private bool _isDataAdapterVisible;

        [ObservableProperty]
        private ObservableCollection<GridConfig> _moveParentOptions = [];

        private SproutControlConfig _moveSourceNode;

        public ObservableCollection<SproutPageConfiguration> NonMenuPages { get; set; }

        public EditPageVM(IConfigurationService configService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _configService = configService;
            _navigationService = navigationService;
            _dialogService = dialogService;


            var nonMenuPages = _configService.Load().Pages.Where(p => p.AddToMenu == false);
            NonMenuPages = new ObservableCollection<SproutPageConfiguration>(nonMenuPages);
            NonMenuPages.Insert(0, new SproutPageConfiguration { Title = "NULL", ID = Guid.Empty });
        }

        public void Initialize(SproutPageConfiguration pageConfig)
        {
            PageConfig = pageConfig.Clone();

            if (PageConfig.Root is not GridConfig gridConfig)
            {
                return;
            }

            Controls.Add(gridConfig);
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                var sproutConfig = _configService.Load();
                var foundPage = sproutConfig.Pages.FirstOrDefault(p => p.Title == PageConfig.Title);
                var pageIndex = sproutConfig.Pages.IndexOf(foundPage);
                sproutConfig.Pages.Remove(foundPage);
                sproutConfig.Pages.Insert(pageIndex, PageConfig);

                _configService.Save(sproutConfig);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }

        [RelayCommand]
        private void AddControl()
        {
            try
            {
                SproutControlConfig newControl = _navigationService.ShowAddControl();

                if (newControl == null) return;

#warning create interface to mark containers
                if (SelectedNode is GridConfig gridConfig)
                {
                    gridConfig.Children.Add(newControl);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }

        [RelayCommand]
        private void RemoveControl()
        {
            try
            {
                if (SelectedNode == null) return;

                var parent = FindParent(Controls, SelectedNode);

                if (parent == null) return;

                parent.Children.Remove(SelectedNode);
                SelectedNode = null;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }

        private static GridConfig FindParent(IEnumerable<SproutControlConfig> nodes, SproutControlConfig target)
        {
            foreach (var node in nodes)
            {
                if (node is GridConfig gridConfig)
                {
                    if (gridConfig.Children.Contains(target))
                    {
                        return gridConfig;
                    }

                    var result = FindParent(gridConfig.Children, target);
                    if (result != null) return result;
                }
            }

            return null;
        }

        public void PrepareMove(SproutControlConfig source)
        {
            _moveSourceNode = source;
            MoveParentOptions.Clear();

            if (source == null) return;

            // Don't allow moving the root node
            if (FindParent(Controls, source) == null) return;

            foreach (var grid in GetAllGridConfigs(Controls))
            {
                // Can't move a node into itself
                if (source == grid) continue;

                // Can't move a GridConfig into one of its own descendants
                if (source is GridConfig sourceGrid && IsDescendant(sourceGrid, grid)) continue;

                MoveParentOptions.Add(grid);
            }
        }

        [RelayCommand]
        private void MoveUp()
        {
            try
            {
                if (SelectedNode == null) return;

                var parent = FindParent(Controls, SelectedNode);
                if (parent == null) return;

                var index = parent.Children.IndexOf(SelectedNode);
                if (index <= 0) return;

                parent.Children.Move(index, index - 1);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }

        [RelayCommand]
        private void MoveDown()
        {
            try
            {
                if (SelectedNode == null) return;

                var parent = FindParent(Controls, SelectedNode);
                if (parent == null) return;

                var index = parent.Children.IndexOf(SelectedNode);
                if (index < 0 || index >= parent.Children.Count - 1) return;

                parent.Children.Move(index, index + 1);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }

        [RelayCommand]
        private void MoveToParent(GridConfig newParent)
        {
            try
            {
                if (_moveSourceNode == null || newParent == null) return;

                var currentParent = FindParent(Controls, _moveSourceNode);
                if (currentParent == null || currentParent == newParent) return;

                currentParent.Children.Remove(_moveSourceNode);
                newParent.Children.Add(_moveSourceNode);
                SelectedNode = _moveSourceNode;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }

        private static IEnumerable<GridConfig> GetAllGridConfigs(IEnumerable<SproutControlConfig> nodes)
        {
            foreach (var node in nodes)
            {
                if (node is GridConfig gridConfig)
                {
                    yield return gridConfig;

                    foreach (var childGrid in GetAllGridConfigs(gridConfig.Children))
                    {
                        yield return childGrid;
                    }
                }
            }
        }

        private static bool IsDescendant(GridConfig root, SproutControlConfig target)
        {
            foreach (var child in root.Children)
            {
                if (child == target) return true;

                if (child is GridConfig childGrid && IsDescendant(childGrid, target))
                    return true;
            }

            return false;
        }

        partial void OnSelectedNodeChanged(SproutControlConfig value)
        {
            try
            {
                if (value is SproutDataGridConfig dataGridConfig && dataGridConfig.DataAdapter != null)
                {
                    AreFiltersVisible = true;
                    SelectedDataGrid = dataGridConfig;
                }
                else
                {
                    AreFiltersVisible = false;
                    SelectedDataGrid = null;
                }

                if (value is SproutButtonConfig sproutButtonConfig)
                {
                    SelectedButton = sproutButtonConfig;
                }
                else
                {
                    SelectedButton = null;
                    SelectedButtonAction = null;
                }

                if (value is GridConfig gridConfig)
                {
                    SelectedGridConfig = gridConfig;
                }
                else
                {
                    SelectedGridConfig = null;
                }

                if (value is IDataAdapterConfigHost dataAdapterControlConfig)
                {
                    IsDataAdapterVisible = true;
                    SelectedDataAdapter = dataAdapterControlConfig.DataAdapter;

                    if (dataAdapterControlConfig.DataAdapter is SqlServerDataAdapterConfig sqlServerDataAdapterConfig)
                    {
                        SelectedAdapterType = "SqlServer";
                        SelectedAdapterViewModel = new SqlServerDataAdapterVM(sqlServerDataAdapterConfig);
                    }
                    else if (dataAdapterControlConfig.DataAdapter is DuckDataAdapterConfig duckDataAdapterConfig)
                    {
                        SelectedAdapterType = "Duck";
                        SelectedAdapterViewModel = new DuckDataAdapterVM(duckDataAdapterConfig);
                    }
                    else
                    {
                        SelectedAdapterType = null;
                        SelectedAdapterViewModel = null;
                    }
                }
                else
                {
                    IsDataAdapterVisible = false;
                    SelectedAdapterType = null;
                    SelectedAdapterViewModel = null;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }

        [RelayCommand]
        private void ChangeAdapter()
        {
            try
            {
                if (SelectedNode is not IDataAdapterConfigHost adapterControl) return;

                if (SelectedAdapterType == "SqlServer")
                {
                    adapterControl.DataAdapter = new SqlServerDataAdapterConfig
                    {
                        DataProvider = new SqlServerDataProviderConfig
                        {
                            Text = string.Empty
                        },

                        InsertCommand = new SqlServerEditCommandConfig(),
                        UpdateCommand = new SqlServerEditCommandConfig(),
                        DeleteCommand = new SqlServerEditCommandConfig(),
                    };
                }
                else if (SelectedAdapterType == "Duck")
                {
                    adapterControl.DataAdapter = new DuckDataAdapterConfig
                    {
                        ConnectionString = "DataSource=:memory:",

                        DataProvider = new DuckDataProviderConfig
                        {
                            Text = string.Empty
                        },

                        InsertCommand = new DuckEditCommandConfig(),
                        UpdateCommand = new DuckEditCommandConfig(),
                        DeleteCommand = new DuckEditCommandConfig(),
                    };
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }

        [RelayCommand]
        private void AddGridRow()
        {
            if (SelectedGridConfig == null) return;

            SelectedGridConfig.Rows.Add("*");
        }

        [RelayCommand]
        private void RemoveGridRow(StringItem stringItem)
        {
            try
            {
                //if (SelectedGridConfig == null || string.IsNullOrEmpty(row)) return;

                SelectedGridConfig.Rows.Remove(stringItem);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }

        [RelayCommand]
        private void AddGridColumn()
        {
            try
            {
                if (SelectedGridConfig == null) return;

                SelectedGridConfig.Columns.Add("*");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }

        [RelayCommand]
        private void RemoveGridColumn(StringItem stringItem)
        {
            try
            {
                //if (SelectedGridConfig == null || string.IsNullOrEmpty(column)) return;

                SelectedGridConfig.Columns.Remove(stringItem);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }
    }
}
