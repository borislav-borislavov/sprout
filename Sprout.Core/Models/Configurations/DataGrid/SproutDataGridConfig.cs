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
	public class SproutDataGridConfig : SproutControlConfig, IDataAdapterControlConfig
	{
		public IDataAdapterConfig DataAdapter { get; set; }

		public bool AllowInsert => DataAdapter?.InsertCommand != null;
		public bool AllowUpdate => DataAdapter?.UpdateCommand != null;
		public bool AllowDelete => DataAdapter?.DeleteCommand != null;

		public bool ShowSave => AllowInsert || AllowUpdate || AllowDelete;

		public ObservableCollection<SproutDataGridColumnConfig> Columns { get; set; } = [];
	}
}
