using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.DataAdapters.Filters;
using Sprout.Core.Models.GridActions;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services;
using Sprout.Core.Services.Queries;
using Sprout.Core.UIStates;
using Sprout.Core.ViewModels;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sprout.Core.Views
{
    public partial class SproutPage : UserControl
    {
        public SproutPage()
        {
            InitializeComponent();
        }

        private bool _isInitialized = false;

        private void InitializePage(SproutPageVM vm)
        {
            if (_isInitialized)
            {
                return;
            }

            Dictionary<string, UIElement> _controls = [];

            //step 1 - generate UI controls
            this.Content = SproutControlFactory.GetControl(vm.PageConfig.Root, _controls);

            //step 2 - hook up control bindings (move this to a better place)
            foreach (var kvp in _controls)
            {
                if (kvp.Value is SproutDataGrid sproutDataGrid)
                {
                    if (!string.IsNullOrEmpty(sproutDataGrid.Name))
                    {
                        sproutDataGrid.dataGrid.SetBinding(DataGrid.ItemsSourceProperty,
                            new Binding($"DataProviders[{sproutDataGrid.Name}].Data")
                            {
                                Mode = BindingMode.OneWay
                            });
                    }

                    //initialize grid actions
                    vm.GridActions.Add(sproutDataGrid.Name, []);

                    sproutDataGrid.btnRefresh.SetBinding(Button.CommandProperty,
                        new Binding(nameof(SproutPageVM.PerformActionCommand))
                        {
                            Mode = BindingMode.OneWay
                        });

                    //create the grid action
                    vm.GridActions[sproutDataGrid.Name][nameof(RefreshDataGridAction)] = new RefreshDataGridAction(sproutDataGrid.Name);

                    sproutDataGrid.btnRefresh.SetBinding(Button.CommandParameterProperty,
                        new Binding($"{nameof(SproutPageVM.GridActions)}[{sproutDataGrid.Name}][{nameof(RefreshDataGridAction)}]")
                        {
                            Mode = BindingMode.OneWay
                        });

                    if (sproutDataGrid.Config.AllowInsert)
                    {
                        sproutDataGrid.btnInsert.SetBinding(Button.CommandProperty,
                            new Binding(nameof(SproutPageVM.PerformActionCommand))
                            {
                                Mode = BindingMode.OneWay
                            });

                        //create the grid action
                        vm.GridActions[sproutDataGrid.Name][nameof(AddRowGridAction)] = new AddRowGridAction(sproutDataGrid.Name);


                        //bind to the newly created grid action
                        sproutDataGrid.btnInsert.SetBinding(Button.CommandParameterProperty,
                            new Binding($"{nameof(SproutPageVM.GridActions)}[{sproutDataGrid.Name}][{nameof(AddRowGridAction)}]")
                            {
                                Mode = BindingMode.OneWay
                            });
                    }

                    if (sproutDataGrid.Config.AllowDelete)
                    {
                        sproutDataGrid.btnDelete.SetBinding(Button.CommandProperty,
                                    new Binding(nameof(SproutPageVM.PerformActionCommand))
                                    {
                                        Mode = BindingMode.OneWay
                                    });

                        //create the grid action
                        vm.GridActions[sproutDataGrid.Name][nameof(MarkDeletedGridAction)] = new MarkDeletedGridAction(sproutDataGrid.Name);

                        //bind to the newly created grid action
                        sproutDataGrid.btnDelete.SetBinding(Button.CommandParameterProperty,
                            new Binding($"{nameof(SproutPageVM.GridActions)}[{sproutDataGrid.Name}][{nameof(MarkDeletedGridAction)}]")
                            {
                                Mode = BindingMode.OneWay
                            });
                    }

                    if (sproutDataGrid.Config.ShowSave)
                    {
                        sproutDataGrid.btnSave.SetBinding(Button.CommandProperty,
                            new Binding(nameof(SproutPageVM.PerformActionCommand))
                            {
                                Mode = BindingMode.OneWay
                            });

                        vm.GridActions[sproutDataGrid.Name][nameof(SaveGridAction)] = new SaveGridAction(sproutDataGrid.Config.Name);

                        sproutDataGrid.btnSave.SetBinding(Button.CommandParameterProperty,
                              new Binding($"{nameof(SproutPageVM.GridActions)}[{sproutDataGrid.Name}][{nameof(SaveGridAction)}]")
                              {
                                  Mode = BindingMode.OneWay
                              });
                    }

                    if (sproutDataGrid.Config.DataAdapter != null)
                    {
                        if (sproutDataGrid.Config.DataAdapter is SqlServerDataAdapterConfig sqlServerDataAdapterConfig)
                        {
                            var dataProvider = sqlServerDataAdapterConfig.DataProvider as SqlServerDataProviderConfig;

                            if (dataProvider.FilterConfigs.Any())
                            {
                                //i should add a general apply filters button the dataGrid UI
                                foreach (var filterConfig in dataProvider.FilterConfigs)
                                {
                                    //UI
                                    var filterView = SproutDataGridFilterFactory.GetFilter(filterConfig);

                                    sproutDataGrid.spFilters.Children.Add(filterView);

                                    var filter = vm.DataProviders[sproutDataGrid.Name].Filters[filterConfig.Title];

                                    if (filterView is SproutDataGridTextFilter textFilter)
                                    {
                                        textFilter.tbFilterValue.SetBinding(TextBox.TextProperty,
                                            new Binding($"DataProviders[{sproutDataGrid.Name}].Filters[{filterConfig.Title}].{nameof(IFilter.StartValue)}")
                                            {
                                                Mode = BindingMode.OneWayToSource,
                                                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                            });
                                    }
                                }

                                sproutDataGrid.btnApplyFilters.SetBinding(Button.CommandProperty,
                                    new Binding(nameof(SproutPageVM.FilterCommand))
                                    {
                                        Mode = BindingMode.OneWay
                                    });
                            }
                        }
                        else
                            throw new NotImplementedException();
                    }

                    vm.UiStateRegistry.Register(sproutDataGrid.UIState.Name, sproutDataGrid.UIState);
                }

                if (kvp.Value is SproutCombo sproutCombo)
                {
                    sproutCombo.comboBox.SetBinding(ComboBox.ItemsSourceProperty,
                        new Binding($"DataProviders[{sproutCombo.Name}].Data")
                        {
                            Mode = BindingMode.OneWay
                        });

                    vm.UiStateRegistry.Register(sproutCombo.UIState.Name, sproutCombo.UIState);
                }
            }
        }

        private void SproutPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is not SproutPageVM vm)
            {
                return;
            }

            InitializePage(vm);

            if (!_isInitialized)
            {
                //step 3 - load the data

                vm.OnLoaded();

                _isInitialized = true;
            }
        }
    }
}
