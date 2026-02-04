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
    public class MarkDeletedGridAction : GridAction
    {
        private readonly string _ownControlName;

        public MarkDeletedGridAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public override void Perform(Dictionary<string, Sprout.Core.Models.DataAdapters.IDataAdapter> dataProviders, UiStateRegistry uiStateRegistry)
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
            {
                return;
            }

            if (selectedRowView.Row.RowState != DataRowState.Deleted)
            {
                selectedRowView.Row["_IsDeleted"] = true;
                selectedRowView.Delete();

            }
            else
            {
                
            }
        }
    }
}
