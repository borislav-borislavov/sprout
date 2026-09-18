using Sprout.Core.Factories;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.CPL;
using Sprout.Core.Services.Dialog;
using Sprout.Core.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Sprout.Core.Views
{
    public partial class SproutPage : UserControl
    {
        public Dictionary<string, UIElement> _controls = [];
        private readonly IConfigurationService _configurationService;
        private readonly ISproutControlFactory _sproutControlFactory;
        private readonly IDialogService _dialogService;

        public SproutPage(IConfigurationService configurationService, ISproutControlFactory sproutControlFactory, IDialogService dialogService)
        {
            InitializeComponent();
            _configurationService = configurationService;
            _sproutControlFactory = sproutControlFactory;
            _dialogService = dialogService;
        }

        public void InitializeControls(SproutPageVM vm)
        {
            //step 1 - generate UI controls
            this.Content = _sproutControlFactory.GetControl(vm.PageConfig.Root, _controls, vm.VMRegistry, vm.PageConfig.ID);

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
                _dialogService.ShowError(ex.ToString());
            }
        }

    }
}
