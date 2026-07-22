using Sprout.Core.Common;
using Sprout.Core.Factories;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.SproutControlVMs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sprout.Core.Features.ButtonActions.GridActions
{
    public class MarkDeletedGridAction : IButtonAction
    {
        private readonly string _ownControlName;

        public MarkDeletedGridAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public Task Perform(VMRegistry vmRegistry, IDataServiceFactory dataServiceFactory)
        {
            var gridVm = vmRegistry.Get<SproutDataGridVM>(_ownControlName);

            if (gridVm == null)
                throw new Exception($"Failed to find SproutDataGridUIState for {_ownControlName}");

            if (gridVm.Selected is not DataRowView selectedRowView)
                return Task.CompletedTask;

            if (selectedRowView.Row[Const.BuiltInDataTableColumns._IsDeleted] is not bool isDeleted)
                return Task.CompletedTask;

            selectedRowView.Row[Const.BuiltInDataTableColumns._IsDeleted] = !isDeleted;

            return Task.CompletedTask;
        }
    }
}
