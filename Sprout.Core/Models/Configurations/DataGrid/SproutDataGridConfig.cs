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

        public ObservableCollection<SproutDataGridColumnConfig> Columns { get; set; } = [];
	}
}
