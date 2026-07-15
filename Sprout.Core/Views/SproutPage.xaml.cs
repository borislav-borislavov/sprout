using Sprout.Core.Behaviours;
using Sprout.Core.Factories;
using Sprout.Core.Models;
using Sprout.Core.Models.ButtonActions;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.DataAdapters.Filters;
using Sprout.Core.Models.GridActions;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.CPL;
using Sprout.Core.Services.SqlServer;
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
        private bool _isInitialized = false;
        public Dictionary<string, UIElement> _controls = [];
        private readonly IConfigurationService _configurationService;
        private readonly ISproutControlFactory _sproutControlFactory;

        public SproutPage(IConfigurationService configurationService, ISproutControlFactory sproutControlFactory)
        {
            InitializeComponent();
            _configurationService = configurationService;
            _sproutControlFactory = sproutControlFactory;
        }

        public void InitializeControls(SproutPageVM vm)
        {
            //step 1 - generate UI controls
            this.Content = _sproutControlFactory.GetControl(vm.PageConfig.Root, _controls);
        }


        public void InitializePage(SproutPageVM vm)
        {
            try
            {
                if (_isInitialized)
                {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(vm.PageConfig.Script))
                {
                    var cpl = new CustomPageLogic();

                    if (cpl.IsLiveDebug)
                    {
                        vm.CompileResult = new()
                        {
                            IsSuccess = true,
                            LiveDebugPage = cpl
                        };
                    }
                    else
                    {
                        var compiler = new CustomPageLogicCompiler(vm, _configurationService);
                        vm.CompileResult = compiler.Compile();
                    }
                }

                vm.RegisterExtraUIStates();

                //step 2 - hook up control bindings (move this to a better place)
                foreach (var kvp in _controls)
                {
                    if (kvp.Value is SproutDataGrid sproutDataGrid)
                    {
                        if (sproutDataGrid.Config.ItemDisplayPage != Guid.Empty)
                        {
                            sproutDataGrid.dataGrid.IsReadOnly = true;

                            DataGridDoubleClickBehavior.SetDoubleClickCommand(sproutDataGrid.dataGrid, vm.DisplayItemPageCommand);
                            var itemDisplayPageInfo = new ItemDisplayPageInfo
                            {
                                GridName = sproutDataGrid.Name,
                                ItemDisplayPageID = sproutDataGrid.Config.ItemDisplayPage
                            };

                            DataGridDoubleClickBehavior.SetDoubleClickCommandParameter(sproutDataGrid.dataGrid, itemDisplayPageInfo);
                        }

                        //TODO: Move to SproutDataGridFactory
                        if (sproutDataGrid.Config.DataAdapter != null)
                        {
                            var dataProvider = sproutDataGrid.Config.DataAdapter.DataProvider;

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

                            }
                        }

                        vm.UiStateRegistry.Register(sproutDataGrid.VM);

                        vm.RegisterGridColumnLayout(sproutDataGrid.VM);
                    }

                    if (kvp.Value is SproutCombo sproutCombo)
                    {
                        sproutCombo.comboBox.SetBinding(ComboBox.ItemsSourceProperty,
                            new Binding($"DataProviders[{sproutCombo.Name}].Data")
                            {
                                Mode = BindingMode.OneWay
                            });

                        if (!string.IsNullOrEmpty(sproutCombo.Config.SelectedValue))
                        {
                            var dependency = DependencyParser.ParseDependencies(sproutCombo.Config.SelectedValue).FirstOrDefault();

                            if (dependency != null)
                            {
                                sproutCombo.comboBox.SetBinding(
                                    ComboBox.SelectedValueProperty,
                                    new Binding
                                    {
                                        Source = vm.UiStateRegistry,
                                        Path = new PropertyPath($"[{dependency.ControlName}].{dependency.PropertyPath}"),
                                        Mode = BindingMode.TwoWay
                                    });
                            }
                            else if (int.TryParse(sproutCombo.Config.SelectedValue, out var selIdx))
                            {
                                sproutCombo.comboBox.SelectedIndex = selIdx;
                            }
                            else
                            {
                                sproutCombo.comboBox.SelectedValue = sproutCombo.Config.SelectedValue;
                            }
                        }

                        vm.UiStateRegistry.Register(sproutCombo.VM);
                    }

                    if (kvp.Value is SproutTextBox sproutTextBox)
                    {
                        vm.UiStateRegistry.Register(sproutTextBox.VM);
                    }

                    if (kvp.Value is SproutLabel sproutLabel)
                    {
                        if (!string.IsNullOrEmpty(sproutLabel.Config.Binding))
                        {
                            var dependency = DependencyParser.ParseDependencies(sproutLabel.Config.Binding).FirstOrDefault();

                            if (dependency != null)
                            {
                                sproutLabel.textBlock.SetBinding(
                                    TextBlock.TextProperty,
                                    new Binding
                                    {
                                        Source = vm.UiStateRegistry,
                                        Path = new PropertyPath($"[{dependency.ControlName}].{dependency.PropertyPath}"),
                                        Mode = BindingMode.OneWay
                                    });
                            }
                        }

                        vm.UiStateRegistry.Register(sproutLabel.VM);
                    }

                    if (kvp.Value is SproutDatePicker sproutDatePicker)
                    {
                        vm.UiStateRegistry.Register(sproutDatePicker.VM);
                    }

                    if (kvp.Value is SproutButton sproutButton)
                    {
                        vm.UiStateRegistry.Register(sproutButton.VM);
                    }

                    if (kvp.Value is SproutBorder sproutBorder)
                    {
                        vm.UiStateRegistry.Register(sproutBorder.VM);
                    }

                    if (kvp.Value is SproutList sproutList)
                    {
                        if (!string.IsNullOrEmpty(sproutList.Name))
                        {
                            sproutList.SetBinding(SproutList.SourceDataProperty,
                                new Binding($"DataProviders[{sproutList.Name}].Data")
                                {
                                    Mode = BindingMode.OneWay
                                });
                        }

                        vm.UiStateRegistry.Register(sproutList.VM);

                        if (sproutList.Config?.Pages is { Count: > 0 } pageLinks &&
                            !string.IsNullOrEmpty(sproutList.Name))
                        {
                            sproutList.pageLaunchMenuRoot.Items.Clear();

                            foreach (var pageLink in pageLinks)
                            {
                                var menuItem = new MenuItem
                                {
                                    Header = string.IsNullOrWhiteSpace(pageLink.Title)
                                        ? "Open page"
                                        : pageLink.Title,
                                    Command = vm.DisplayListItemPageCommand,
                                    CommandParameter = new ListPageLaunchInfo
                                    {
                                        ListName = sproutList.Name,
                                        PageId = pageLink.PageId
                                    }
                                };

                                sproutList.pageLaunchMenuRoot.Items.Add(menuItem);
                            }

                            sproutList.pageLaunchMenu.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            sproutList.pageLaunchMenu.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

    }
}
