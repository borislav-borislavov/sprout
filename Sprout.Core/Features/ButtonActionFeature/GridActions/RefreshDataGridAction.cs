using Sprout.Core.Common;
using Sprout.Core.Factories;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.SproutControlVMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Features.ButtonActions.GridActions
{
    internal class RefreshDataGridAction : IButtonAction
    {
        private readonly string _ownControlName;

        public RefreshDataGridAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public async Task Perform(VMRegistry vmRegistry, IDataServiceFactory dataServiceFactory)
        {
            var ownDataAdapter = vmRegistry.GetAdapterOrThrow(_ownControlName);
            using var dataService = dataServiceFactory.Create(ownDataAdapter, vmRegistry);
            await dataService.ProvideData();
        }
    }
}
