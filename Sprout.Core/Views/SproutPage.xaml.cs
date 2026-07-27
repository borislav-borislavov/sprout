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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

    }
}
