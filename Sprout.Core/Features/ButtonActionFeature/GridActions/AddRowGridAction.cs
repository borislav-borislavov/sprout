using Sprout.Core.Common;
using Sprout.Core.Factories;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.SproutControlVMs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Features.ButtonActions.GridActions
{
    public class AddRowGridAction : IButtonAction
    {
        private readonly string _ownControlName;

        public AddRowGridAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public Task Perform(VMRegistry vmRegistry, IDataServiceFactory dataServiceFactory)
        {
            var ownDataAdapter = vmRegistry.GetAdapterOrThrow(_ownControlName);

            var newRow = ownDataAdapter.DataProvider.Data.NewRow();
            ownDataAdapter.DataProvider.Data.Rows.Add(newRow);

            return Task.CompletedTask;
        }
    }
}
