using Microsoft.Data.SqlClient;
using Sprout.Core.Common;
using Sprout.Core.Common.Models;
using Sprout.Core.Factories;
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
using static Sprout.Core.Services.SqlServer.SqlServerDataService;

namespace Sprout.Core.Models.GridActions
{
	public class SaveGridAction : GridAction
	{
		private readonly string _ownControlName;

		public SaveGridAction(string ownControlName)
		{
			_ownControlName = ownControlName;
		}

		public override async Task Perform(Dictionary<string, Sprout.Core.Models.DataAdapters.IDataAdapter> dataAdapters, UiStateRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
		{
            if (!dataAdapters.TryGetValue(_ownControlName, out var ownDataAdapter))
			{
				//find a nice way to route logs to the screen
				throw new NotImplementedException();
			}

			using (var dataService = dataServiceFactory.Create(ownDataAdapter, uiStateRegistry))
			{
				foreach (System.Data.DataRow dataRow in ownDataAdapter.DataProvider.Data.Rows)
				{
					if (dataRow.RowState == DataRowState.Added)
					{
						await dataService.Insert(dataRow);
					}
					else if (dataRow[nameof(Const.BuiltInDataTableColumns._IsDeleted)] is bool isDeleted && isDeleted)
					{
                        await dataService.Delete(dataRow);
					}
					else if (dataRow.RowState == DataRowState.Modified)
					{
						await dataService.Update(dataRow);
					}
				}

				await dataService.ProvideData(); 
			}
        }
    }
}
