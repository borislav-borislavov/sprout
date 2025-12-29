using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
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
        private Dictionary<string, UIElement> _controls = [];

        public SproutPage()
        {
            InitializeComponent();
        }

        private void InitializePage(SproutPageVM vm)
        {
            //step 1 - generate UI controls
            this.Content = SproutControlFactory.GetControl(vm.PageConfig.Root, _controls);

            //step 2 - hook up control bindings (move this to a better place)
            foreach (var kvp in _controls)
            {
                if (kvp.Value is SproutDataGrid sproutDataGrid)
                {
                    sproutDataGrid.dataGrid.SetBinding(DataGrid.ItemsSourceProperty,
                        new Binding($"Queries[{sproutDataGrid.QueryName}].Data")
                        {
                            Mode = BindingMode.OneWay
                        });

                    //initialize grid actions
                    vm.GridActions.Add(sproutDataGrid.Name, []);

                    if (sproutDataGrid.Config.AllowInsert)
                    {
                        sproutDataGrid.btnInsert.SetBinding(Button.CommandProperty,
                            new Binding(nameof(SproutPageVM.PerformActionCommand))
                            {
                                Mode = BindingMode.OneWay
                            });

                        //create the grid action
                        vm.GridActions[sproutDataGrid.Name][nameof(AddRowGridAction)] = new AddRowGridAction(sproutDataGrid.QueryName);

                        //bind to the newly created grid action
                        sproutDataGrid.btnInsert.SetBinding(Button.CommandParameterProperty,
                            new Binding($"{nameof(SproutPageVM.GridActions)}[{sproutDataGrid.Name}][{nameof(AddRowGridAction)}]")
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

                        vm.GridActions[sproutDataGrid.Name][nameof(SaveGridAction)] = new SaveGridAction(sproutDataGrid.QueryName);

                        sproutDataGrid.btnSave.SetBinding(Button.CommandParameterProperty,
                              new Binding($"{nameof(SproutPageVM.GridActions)}[{sproutDataGrid.Name}][{nameof(SaveGridAction)}]")
                              {
                                  Mode = BindingMode.OneWay
                              });
                    }

                    vm.UiStateRegistry.Register(sproutDataGrid.UIState.Name, sproutDataGrid.UIState);
                }

                if (kvp.Value is SproutCombo sproutCombo)
                {
                    sproutCombo.comboBox.SetBinding(ComboBox.ItemsSourceProperty,
                        new Binding($"Queries[{sproutCombo.QueryName}].Data")
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
            //step 3 - load the data
            ((SproutPageVM)DataContext).OnLoaded();
        }
    }
}
