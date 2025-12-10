using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Factories
{
    public class PageFactory : IPageFactory
    {
        private readonly IServiceProvider provider;

        public PageFactory(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public SproutPage Create(SproutPageConfiguration pageConfig)
        {
            var page = ActivatorUtilities.CreateInstance<SproutPage>(provider, pageConfig);

            return page;
        }
    }
}
