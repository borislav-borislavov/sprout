using Sprout.Core.Factories;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.UIStates;
using Sprout.Core.ViewModels;
using Sprout.Core.Windows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows;

namespace Sprout.Core.Features.ButtonActions.GridActions
{
    /// <summary>
    /// Opens a lightweight, read-only window that previews the currently selected grid row
    /// as a vertical list of ColumnName / Value pairs with a built-in column-name filter.
    /// </summary>
    public class PreviewRowGridAction : IButtonAction
    {
        private readonly string _ownControlName;
        private readonly Services.Clipboard.IClipboardService _clipboardService;

        public PreviewRowGridAction(string ownControlName, Services.Clipboard.IClipboardService clipboardService)
        {
            _ownControlName = ownControlName;
            _clipboardService = clipboardService;
        }

        public Task Perform(Dictionary<string, Models.DataAdapters.IDataAdapter> dataAdapters, UiStateRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
        {
            var gridUiState = uiStateRegistry.Get<SproutDataGridVM>(_ownControlName);

            if (gridUiState == null)
                throw new Exception($"Failed to find SproutDataGridUIState for {_ownControlName}");

            if (gridUiState.Selected is not DataRowView selectedRowView)
            {
                // No selection: if the grid contains exactly one row, preview it anyway.
                if (gridUiState.Grid?.dataGrid?.ItemsSource is DataView dataView && dataView.Count == 1)
                    selectedRowView = dataView[0];
                else
                    return Task.CompletedTask;
            }

            var vm = new RowPreviewVM(selectedRowView, gridUiState.Grid?.Config?.Columns, _clipboardService);
            var window = new RowPreviewWindow(vm)
            {
                Owner = Window.GetWindow(gridUiState.Grid)
            };

            window.Show();

            return Task.CompletedTask;
        }
    }
}
