using Sprout.Core.Common;
using Sprout.Core.Factories;
using Sprout.Core.Models.ButtonActions;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sprout.Core.Models.GridActions
{
    public class MarkDeletedGridAction : IButtonAction
    {
        private readonly string _ownControlName;

        public MarkDeletedGridAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public Task Perform(Dictionary<string, Sprout.Core.Models.DataAdapters.IDataAdapter> dataProviders, UiStateRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
        {
            if (!dataProviders.TryGetValue(_ownControlName, out var ownDataAdapter))
            {
                //find a nice way to route logs to the screen

                throw new NotImplementedException();
            }

            var gridUiState = uiStateRegistry.Get<SproutGridUIState>(_ownControlName);

            if (gridUiState == null)
                throw new Exception($"Failed to find SproutGridUIState for {_ownControlName}");

            if (gridUiState.Selected is not DataRowView selectedRowView)
                return Task.CompletedTask;

            if (selectedRowView.Row[Const.BuiltInDataTableColumns._IsDeleted] is not bool isDeleted)
                return Task.CompletedTask;

            selectedRowView.Row[Const.BuiltInDataTableColumns._IsDeleted] = !isDeleted;

            return Task.CompletedTask;
        }
    }
}
