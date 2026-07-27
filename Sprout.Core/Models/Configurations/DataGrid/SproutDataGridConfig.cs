using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Models.Queries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations.DataGrid
{
	public class SproutDataGridConfig : SproutControlConfig, IDataAdapterConfigHost
	{
		public IDataAdapterConfig DataAdapter { get; set; }

		public bool AllowInsert => !string.IsNullOrEmpty(DataAdapter?.InsertCommand.Text);
		public bool AllowUpdate => !string.IsNullOrEmpty(DataAdapter?.UpdateCommand.Text);
		public bool AllowDelete => !string.IsNullOrEmpty(DataAdapter?.DeleteCommand.Text);

		public bool ShowSave => AllowInsert || AllowUpdate || AllowDelete;

		public Guid ItemDisplayPage { get; set; }

		/// <summary>
		/// When set, the internal data grid is read only and cells cannot be edited.
		/// </summary>
		public bool IsReadOnly { get; set; }

		/// <summary>
		/// Number of detail fields shown per row in the RowDetailsTemplate. Default is 10.
		/// </summary>
		public int RowDetailsItemsPerRow { get; set; } = 10;

		public double? Height { get; set; }

		public double? Width { get; set; }

		public string Margin { get; set; }

		public string HorizontalAlignment { get; set; }

		public string VerticalAlignment { get; set; }

		public string ToolTip { get; set; }

		public ObservableCollection<SproutDataGridColumnConfig> Columns { get; set; } = [];

		/// <summary>
		/// Pages that can be opened for the selected row from the grid's "Row" button.
		/// </summary>
		public ObservableCollection<SproutDataGridRowActionConfig> RowActions { get; set; } = [];
	}

	/// <summary>
	/// A single entry of the grid's "Row" button: opens the given page with the selected row as parameter.
	/// </summary>
	public class SproutDataGridRowActionConfig
	{
		public string Title { get; set; }

		public Guid PageID { get; set; }
	}
}
