using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Models.Configurations;
using Sprout.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Sprout.Core.Factories
{
    public class SproutPageVMFactory(IServiceProvider _serviceProvider) : ISproutPageVMFactory
    {
        public SproutPageVM Create(SproutPageConfiguration pageConfig, object? parameter)
        {
            var vm = ActivatorUtilities.CreateInstance<SproutPageVM>(
                _serviceProvider,
                pageConfig
            );

            if (parameter != null)
            {
                vm.SproutPageUIState.Data = parameter;
            }

            return vm;
        }
    }
}
