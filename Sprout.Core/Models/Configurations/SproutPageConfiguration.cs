using Sprout.Core.Models.Configurations.DataGrid;

namespace Sprout.Core.Models.Configurations
{
	public class SproutPageConfiguration
	{
		public string Title { get; set; }

        public Guid ID { get; set; }

        public bool AddToMenu { get; set; }

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

		private void GetDataAdapterConfigsRecursive(
			SproutControlConfig control, Dictionary<string, IDataAdapterConfig> dataAdapterConfigs)
		{
			if (control is GridConfig grid)
			{
				foreach (var child in grid.Children)
				{
					GetDataAdapterConfigsRecursive(child, dataAdapterConfigs);
				}
			}
			else if (control is IDataAdapterControlConfig dataAdapterControlConfig)
			{
				if (dataAdapterControlConfig.DataAdapter is not null)
				{
					dataAdapterConfigs.Add(control.Name, dataAdapterControlConfig.DataAdapter);
				}

				if (control is SproutDataGridConfig dataGridConfig)
				{
					foreach (var column in dataGridConfig.Columns ?? [])
					{
						if (column.ColumnType == ColumnType.Combo && column.ComboDataAdapter is not null)
						{
                            column.ComboAdapterKey = $"{control.Name}.Column.{column.BindingPath}";
							dataAdapterConfigs.Add(column.ComboAdapterKey, column.ComboDataAdapter);
						}
					}
				}
			}
		}
	}
}
