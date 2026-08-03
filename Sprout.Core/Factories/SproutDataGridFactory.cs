using Sprout.Core.Behaviours;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.Features.ButtonActions.GridActions;
using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.DataAdapters.Filters;
using Sprout.Core.Services.Clipboard;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Views;
using Sprout.Core.Views.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Sprout.Core.Factories
{
    public class SproutDataGridFactory : BaseSproutControlFactory, ISproutDataGridFactory
    {
        private readonly IClipboardService _clipboardService;
        private readonly IDataAdapterFactory _dataAdapterFactory;
        private readonly IConfigurationService _configurationService;
        private readonly IDialogService _dialogService;

        public SproutDataGridFactory(IClipboardService clipboardService, IDataAdapterFactory dataAdapterFactory, IConfigurationService configurationService, IDialogService dialogService)
        {
            _clipboardService = clipboardService;
            _dataAdapterFactory = dataAdapterFactory;
            _configurationService = configurationService;
            _dialogService = dialogService;
        }

        public SproutDataGrid Create(SproutDataGridConfig sproutGridConfig)
        {
            var sproutDataGrid = new SproutDataGrid
            {
                Name = sproutGridConfig.Name,
                Config = sproutGridConfig,
                VM = new SproutDataGridVM(sproutGridConfig.Name, _configurationService, _dialogService)
            };
            sproutDataGrid.VM.Grid = sproutDataGrid;

            sproutDataGrid.dataGrid.IsReadOnly = sproutGridConfig.IsReadOnly;

            var columnFactory = new SproutDataGridColumnFactory(_dataAdapterFactory, sproutDataGrid);

            foreach (var colConfig in (sproutGridConfig.Columns ?? []).Where(c => !c.ShowInRowDetails))
            {
                var col = columnFactory.Create(colConfig);
                sproutDataGrid.dataGrid.Columns.Add(col);
                sproutDataGrid.ColumnKeys[col] = colConfig.BindingPath ?? colConfig.Header;
            }

            if (sproutDataGrid.dataGrid.Columns.Count == 0)
            {
                sproutDataGrid.dataGrid.AutoGenerateColumns = true;
            }

            var rowDetailColumns = (sproutGridConfig.Columns ?? []).Where(c => c.ShowInRowDetails).ToList();
            if (rowDetailColumns.Count > 0)
            {
                sproutDataGrid.dataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
                sproutDataGrid.dataGrid.RowDetailsTemplate = BuildRowDetailsTemplate(rowDetailColumns, sproutGridConfig.RowDetailsItemsPerRow);
            }

            SetPositionInGrid(sproutDataGrid, sproutGridConfig);

            if (sproutGridConfig.Height.HasValue)
                sproutDataGrid.dataGrid.Height = sproutGridConfig.Height.Value;

            if (sproutGridConfig.Width.HasValue)
                sproutDataGrid.dataGrid.Width = sproutGridConfig.Width.Value;

            if (!string.IsNullOrWhiteSpace(sproutGridConfig.Margin))
            {
                if (new ThicknessConverter().ConvertFromString(sproutGridConfig.Margin) is Thickness margin)
                    sproutDataGrid.Margin = margin;
            }

            if (!string.IsNullOrEmpty(sproutGridConfig.HorizontalAlignment) &&
                sproutGridConfig.HorizontalAlignment != "(Default)" &&
                Enum.TryParse<HorizontalAlignment>(sproutGridConfig.HorizontalAlignment, out var hAlign))
            {
                sproutDataGrid.HorizontalAlignment = hAlign;
            }

            if (!string.IsNullOrEmpty(sproutGridConfig.VerticalAlignment) &&
                sproutGridConfig.VerticalAlignment != "(Default)" &&
                Enum.TryParse<VerticalAlignment>(sproutGridConfig.VerticalAlignment, out var vAlign))
            {
                sproutDataGrid.VerticalAlignment = vAlign;
            }

            if (!string.IsNullOrEmpty(sproutGridConfig.ToolTip))
                sproutDataGrid.ToolTip = sproutGridConfig.ToolTip;

            #region Set up which page opens on double click
            if (sproutDataGrid.Config.ItemDisplayPage != Guid.Empty)
            {
                DataGridDoubleClickBehavior.SetDoubleClickCommand(sproutDataGrid.dataGrid, sproutDataGrid.VM.DisplayItemPageCommand);
                var itemDisplayPageInfo = new ItemDisplayPageInfo
                {
                    GridName = sproutDataGrid.Name,
                    ItemDisplayPageID = sproutDataGrid.Config.ItemDisplayPage
                };

                DataGridDoubleClickBehavior.SetDoubleClickCommandParameter(sproutDataGrid.dataGrid, itemDisplayPageInfo);
            } 
            #endregion

            #region Set up the Row button pages
            if (sproutDataGrid.Config.RowActions.Any(ra => ra.PageID != Guid.Empty))
            {
                sproutDataGrid.menuRow.Visibility = Visibility.Visible;

                var rowContextMenu = new ContextMenu();

                foreach (var rowAction in sproutDataGrid.Config.RowActions.Where(ra => ra.PageID != Guid.Empty))
                {
                    sproutDataGrid.menuRowRoot.Items.Add(new MenuItem
                    {
                        Header = rowAction.Title,
                        Command = sproutDataGrid.VM.OpenRowActionPageCommand,
                        CommandParameter = rowAction
                    });

                    rowContextMenu.Items.Add(new MenuItem
                    {
                        Header = rowAction.Title,
                        Command = sproutDataGrid.VM.OpenRowActionPageCommand,
                        CommandParameter = rowAction
                    });
                }

                sproutDataGrid.dataGrid.ContextMenu = rowContextMenu;

                //Right click does not select the row by default, so the context menu would
                //act on a stale selection. Select the row under the cursor before it opens.
                sproutDataGrid.dataGrid.PreviewMouseRightButtonDown += (s, e) =>
                {
                    var source = e.OriginalSource as DependencyObject;

                    while (source != null && source is not DataGridRow)
                        source = VisualTreeHelper.GetParent(source);

                    if (source is DataGridRow row)
                    {
                        row.IsSelected = true;
                    }
                };
            }
            #endregion

            if (sproutDataGrid.Config.DataAdapter != null)
            {
                sproutDataGrid.VM.DataAdapter ??= _dataAdapterFactory.Create(sproutDataGrid.Config.DataAdapter);

                var dataProvider = sproutDataGrid.Config.DataAdapter.DataProvider;

                if (dataProvider.FilterConfigs.Any())
                {
                    //The adapter must exist here so the filter views can bind to its filter
                    //instances. SproutControlFactory skips creation when it is already set.
                    //sproutDataGrid.VM.DataAdapter ??= _dataAdapterFactory.Create(sproutDataGrid.Config.DataAdapter);

                    //i should add a general apply filters button the dataGrid UI
                    foreach (var filterConfig in dataProvider.FilterConfigs)
                    {
                        //UI
                        var filterView = SproutDataGridFilterFactory.GetFilter(filterConfig);

                        sproutDataGrid.spFilters.Children.Add(filterView);

                        var filter = sproutDataGrid.VM.DataAdapter.DataProvider.Filters[filterConfig.Title];

                        if (filterView is SproutDataGridTextFilter textFilter)
                        {
                            textFilter.tbFilterValue.SetBinding(TextBox.TextProperty,
                                new Binding(nameof(IFilter.StartValue))
                                {
                                    Source = filter,
                                    Mode = BindingMode.OneWayToSource,
                                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                });
                        }
                    }
                }
                else
                {
                    sproutDataGrid.btnFilters.Visibility = Visibility.Collapsed;
                    sproutDataGrid.filtersSeparator.Visibility = Visibility.Collapsed;
                }
            }

            SetupVM(sproutDataGrid);

            sproutDataGrid.VM.RegisterGridColumnLayout();

            return sproutDataGrid;
        }

        private void SetupVM(SproutDataGrid sproutDataGrid)
        {
            sproutDataGrid.VM.SetUpState(sproutDataGrid); //currenlty binds the dataGrid to its DataSource, probably should be moved here?

            #region Bind ItemsSource
            sproutDataGrid.dataGrid.SetBinding(DataGrid.ItemsSourceProperty,
                new Binding()
                {
                    Mode = BindingMode.OneWay,
                    Source = sproutDataGrid.VM,
                    Path = new PropertyPath("DataAdapter.DataProvider.Data")
                });
            #endregion

            #region Bind buttons
            sproutDataGrid.BindButtonAction(sproutDataGrid.btnRefresh, new RefreshDataGridAction(sproutDataGrid.Name));
            sproutDataGrid.BindButtonAction(sproutDataGrid.btnApplyFilters, new RefreshDataGridAction(sproutDataGrid.Name));
            sproutDataGrid.BindButtonAction(sproutDataGrid.menuExportExcel, new ExportToExcelGridAction(sproutDataGrid.Name));
            sproutDataGrid.BindButtonAction(sproutDataGrid.btnRowPreview, new PreviewRowGridAction(sproutDataGrid.Name, _clipboardService));
            sproutDataGrid.BindButtonAction(sproutDataGrid.btnLocalSearch, new LocalSearchGridAction(sproutDataGrid.Name));

            if (sproutDataGrid.Config.AllowInsert)
            {
                sproutDataGrid.BindButtonAction(sproutDataGrid.btnInsert, new AddRowGridAction(sproutDataGrid.Name));
            }

            if (sproutDataGrid.Config.AllowDelete)
            {
                sproutDataGrid.BindButtonAction(sproutDataGrid.btnDelete, new MarkDeletedGridAction(sproutDataGrid.Name));
            }

            if (sproutDataGrid.Config.ShowSave)
            {
                sproutDataGrid.BindButtonAction(sproutDataGrid.btnSave, new SaveGridAction(sproutDataGrid.Name));
            } 
            #endregion
        }

        private static DataTemplate BuildRowDetailsTemplate(List<SproutDataGridColumnConfig> columns, int itemsPerRow)
        {
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(245, 245, 245)));
            border.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Colors.LightGray));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
            border.SetValue(Border.PaddingProperty, new Thickness(8, 4, 8, 4));

            int columns_ = Math.Max(1, itemsPerRow);

            var uniformGrid = new FrameworkElementFactory(typeof(UniformGrid));
            uniformGrid.SetValue(UniformGrid.ColumnsProperty, columns_);

            foreach (var col in columns)
            {
                var entryPanel = new FrameworkElementFactory(typeof(StackPanel));
                entryPanel.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
                entryPanel.SetValue(StackPanel.MarginProperty, new Thickness(0, 2, 16, 2));

                var header = new FrameworkElementFactory(typeof(TextBlock));
                header.SetValue(TextBlock.TextProperty, (col.Header ?? col.BindingPath) + ": ");
                header.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
                header.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.DarkSlateGray));

                var value = new FrameworkElementFactory(typeof(TextBlock));
                value.SetBinding(TextBlock.TextProperty, new Binding(col.BindingPath));
                value.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.DarkSlateGray));

                entryPanel.AppendChild(header);
                entryPanel.AppendChild(value);
                uniformGrid.AppendChild(entryPanel);
            }

            border.AppendChild(uniformGrid);

            return new DataTemplate { VisualTree = border };
        }
    }
}
