using Microsoft.Data.SqlClient;
using Sprout.Core.Common;
using Sprout.Core.Common.Models;
using Sprout.Core.Factories;
using Sprout.Core.Models.ButtonActions;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.UIStates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using static Sprout.Core.Services.SqlServer.SqlServerDataService;

namespace Sprout.Core.Models.GridActions
{
    public class SaveGridAction : IButtonAction, IButtonActionMessenger
    {
        private readonly string _ownControlName;

        public SaveGridAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public List<ActionMessage> Messages => [];

        public async Task Perform(Dictionary<string, Sprout.Core.Models.DataAdapters.IDataAdapter> dataAdapters, UiStateRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
        {
            ResetMessages();

            if (!dataAdapters.TryGetValue(_ownControlName, out var ownDataAdapter))
            {
                //find a nice way to route logs to the screen
                throw new NotImplementedException();
            }

            List<ActionMessage> allMessages = [];

            using (var dataService = dataServiceFactory.Create(ownDataAdapter, uiStateRegistry))
            {
                IEnumerable<ActionMessage> msgs = [];

                foreach (System.Data.DataRow dataRow in ownDataAdapter.DataProvider.Data.Rows)
                {
                    if (dataRow.RowState == DataRowState.Added)
                    {
                        msgs = await dataService.Insert(dataRow);
                    }
                    else if (dataRow[nameof(Const.BuiltInDataTableColumns._IsDeleted)] is bool isDeleted && isDeleted)
                    {
                        msgs = await dataService.Delete(dataRow);
                    }
                    else if (dataRow.RowState == DataRowState.Modified)
                    {
                        msgs = await dataService.Update(dataRow);
                    }

                    if (msgs.Any())
                        allMessages.AddRange(msgs);
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
