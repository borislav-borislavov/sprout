using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Messages;
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
        public SproutPageVM Create(SproutPageConfiguration pageConfig, OpenTabMessageArgs? args)
        {
            //This is needed so that ActivatorUtilities.CreateInstance works properly
            if (args == null) args = new();

            var vm = ActivatorUtilities.CreateInstance<SproutPageVM>(
                _serviceProvider,
                pageConfig,
                args
            );

            return vm;
        }
    }
}
