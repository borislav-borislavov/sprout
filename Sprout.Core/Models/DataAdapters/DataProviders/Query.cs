using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.DataAdapters.DataProviders
{
	public partial class SqlServerDataProvider : ObservableObject, IDataProvider
	{
		public string Name { get; set; }

		/// <summary>
		/// The Root table name of the query. Used for automatic updates.
		/// </summary>
		public string TableName { get; set; }
		public List<string> PrimaryKeys { get; set; } = [];
		public string Text { get; set; }

		[ObservableProperty]
		private DataTable _data = new();

		private SqlServerDataAdapter _parentAdapter;

        public SqlServerDataProvider(SqlServerDataAdapter parentAdapter)
        {
            _parentAdapter = parentAdapter;
        }

        partial void OnDataChanged(DataTable value)
		{
			if (value == null) return;

			value.ColumnChanged += Data_ColumnChanged;
		}

		private void Data_ColumnChanged(object sender, DataColumnChangeEventArgs e)
		{
			var row = e.Row;
			var column = e.Column!.ColumnName;
			var newValue = e.ProposedValue;
			var oldValue = row[column, DataRowVersion.Original];

			// Guard: ignore unchanged values
			if (Equals(oldValue, newValue))
				return;

#warning left off here
			//avoid the granular updates, use bulk changes instead

			//UpdateDatabase(
			//    id: row["Id"],
			//    column: column,
			//    value: newValue
			//);
		}

		public string ConnectionString => _parentAdapter.ConnectionString;


        public IEnumerable<DataProviderDependency> Dependencies { get; internal set; } = [];
	}
}
