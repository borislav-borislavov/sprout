using Sprout.Core.Models.Configurations.DataGrid;

namespace Sprout.Core.Models.Configurations
{
	public class SproutPageConfiguration
	{
		public string Title { get; set; }

		public Guid ID { get; set; }

		public bool AddToMenu { get; set; }

        //"\uE74E"
        public string Icon { get; set; }
		public string IconFont { get; set; } = "Segoe MDL2 Assets";

        public Guid? CategoryID { get; set; }

        public string Script { get; set; }

        // Extra namespaces imported into this page's script (in addition to the
        // compiler's built-in defaults). Used for both compilation and completion.
        public List<string> Usings { get; set; } = [];

        public SproutControlConfig Root { get; set; }

		public Dictionary<string, IDataAdapterConfig> GetDataAdapterConfigs()
		{
			Dictionary<string, IDataAdapterConfig> dataAdapterConfigs = [];

			if (Root is null || Root is not GridConfig rootGridConfig)
			{
				return dataAdapterConfigs;
			}

			GetDataAdapterConfigsRecursive(Root, dataAdapterConfigs);

			return dataAdapterConfigs;
		}

		private void GetDataAdapterConfigsRecursive(SproutControlConfig control, Dictionary<string, IDataAdapterConfig> dataAdapterConfigs)
		{
			if (control is GridConfig grid)
			{
				foreach (var child in grid.Children)
				{
					GetDataAdapterConfigsRecursive(child, dataAdapterConfigs);
				}
			}
			else if (control is IDataAdapterConfigHost dataAdapterControlConfig)
			{
				if (dataAdapterControlConfig.DataAdapter is not null)
				{
                    //this helps the DataAdapter fetch the correct UIState for the control it's associated with
                    dataAdapterControlConfig.DataAdapter.ParentType = control.GetType();
                    dataAdapterControlConfig.DataAdapter.Name = control.Name;

                    dataAdapterConfigs.Add(control.Name, dataAdapterControlConfig.DataAdapter);
				}

				if (control is SproutDataGridConfig dataGridConfig)
				{
					foreach (var column in dataGridConfig.Columns ?? [])
					{
						if (column.ColumnType == ColumnType.Combo && column.DataAdapter is not null)
						{
                            //this helps the DataAdapter fetch the correct UIState for the control it's associated with
                            column.DataAdapter.ParentType = column.GetType();
                            column.DataAdapter.Name = column.DisplayColumn;

                            column.ComboAdapterKey = $"{control.Name}.Column.{column.BindingPath}";
							dataAdapterConfigs.Add(column.ComboAdapterKey, column.DataAdapter);
						}
					}
				}
			}
		}
	}
}
