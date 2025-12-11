using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Menus;
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
        private readonly IServiceProvider _provider;

        public PageFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public SproutPage Create(SproutPageConfiguration pageConfig)
        {
            var page = ActivatorUtilities.CreateInstance<SproutPage>(_provider, pageConfig);

            return page;
        }
    }
}
