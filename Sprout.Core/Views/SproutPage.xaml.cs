using Sprout.Core.Behaviours;
using Sprout.Core.Factories;
using Sprout.Core.Models;
using Sprout.Core.Models.DataAdapters.Filters;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.CPL;
using Sprout.Core.ViewModels;
using Sprout.Core.Views.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
            this.Content = _sproutControlFactory.GetControl(vm.PageConfig.Root, _controls, vm.VMRegistry);

            //step 1.1 Register extra VMs (all other VMs are registered in the SproutControlFactory)
            vm.RegisterExtraVMs();
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

                //step 2 - hook up registry path bindings (labels, two way text boxes).
                //Runs after all VMs are registered so the registry indexer path resolves
                //regardless of the order the controls appear on the page.
                foreach (var kvp in _controls)
                {
                    if (kvp.Value is SproutLabel sproutLabel &&
                        !string.IsNullOrEmpty(sproutLabel.Config.Binding))
                    {
                        var dependency = DependencyParser.ParseDependencies(sproutLabel.Config.Binding).FirstOrDefault();

                        if (dependency != null)
                        {
                            sproutLabel.textBlock.SetBinding(
                                TextBlock.TextProperty,
                                new Binding
                                {
                                    Source = vm.VMRegistry,
                                    Path = new PropertyPath($"[{dependency.ControlName}].{dependency.PropertyPath}"),
                                    Mode = BindingMode.OneWay
                                });
                        }
                    }

                    if (kvp.Value is SproutTextBox sproutTextBox &&
                        sproutTextBox.Config.TwoWayBinding &&
                        !string.IsNullOrEmpty(sproutTextBox.Config.Binding))
                    {
                        var dependency = DependencyParser.ParseDependencies(sproutTextBox.Config.Binding).FirstOrDefault();

                        if (dependency != null)
                        {
                            //Replaces the VM.Text binding from SetUpState. The binding engine keeps
                            //both directions in sync and suppresses feedback loops natively.
                            sproutTextBox.textBox.SetBinding(TextBox.TextProperty,
                                new Binding($"[{dependency.ControlName}].{dependency.PropertyPath}")
                                {
                                    Source = vm.VMRegistry,
                                    Mode = BindingMode.TwoWay,
                                    UpdateSourceTrigger = sproutTextBox.Config.ChangeValueOnEnter
                                        ? UpdateSourceTrigger.Explicit
                                        : UpdateSourceTrigger.PropertyChanged
                                });

                            //Keep VM.Text in sync so page logic reading the VM still sees the value.
                            sproutTextBox.textBox.TextChanged += (s, e) =>
                                sproutTextBox.VM.Text = sproutTextBox.textBox.Text;
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
