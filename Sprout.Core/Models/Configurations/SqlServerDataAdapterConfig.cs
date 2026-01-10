using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
	internal class SqlServerDataAdapterConfig : IDataAdapterConfig
	{
		public string ConnectionString { get; set; }

		public IDataProviderConfig DataProvider { get; set; }

		public IEditCommandConfig InsertCommand { get; set; }
		public IEditCommandConfig UpdateCommand { get; set; }
		public IEditCommandConfig DeleteCommand { get; set; }
	}
}
