using Microsoft.Data.SqlClient;
using Sprout.Core.Common;
using Sprout.Core.Common.Models;
using Sprout.Core.Factories;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.Models;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.SproutControlVMs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using static Sprout.Core.Services.SqlServer.SqlServerDataService;

namespace Sprout.Core.Features.ButtonActions.GridActions
{
    public class SaveGridAction : IButtonAction, IButtonActionMessenger
    {
        private readonly string _ownControlName;

        public SaveGridAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public List<ActionMessage> Messages { get; } = [];

        public async Task Perform(VMRegistry vmRegistry, IDataServiceFactory dataServiceFactory)
        {
            ResetMessages();

            var ownDataAdapter = vmRegistry.GetAdapterOrThrow(_ownControlName);

            using (var dataService = dataServiceFactory.Create(ownDataAdapter, vmRegistry))
            {
                ChangeResult changeResult = new();

                foreach (System.Data.DataRow dataRow in ownDataAdapter.DataProvider.Data.Rows)
                {
                    if (dataRow.RowState == DataRowState.Added)
                    {
                        changeResult = await dataService.Insert(dataRow);
                    }
                    else if (dataRow[nameof(Const.BuiltInDataTableColumns._IsDeleted)] is bool isDeleted && isDeleted)
                    {
                        changeResult = await dataService.Delete(dataRow);
                    }
                    else if (dataRow.RowState == DataRowState.Modified)
                    {
                        changeResult = await dataService.Update(dataRow);
                    }

                    if (changeResult.Messages.Any())
                        Messages.AddRange(changeResult.Messages);

                }

                await dataService.ProvideData();
            }
        }

        public void ResetMessages()
        {
            Messages.Clear();
        }
    }
}
