using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
	public class SqlServerDataAdapterConfig : IDataAdapterConfig
	{
		public string ConnectionString { get; set; }

		public IDataProviderConfig DataProvider { get; set; }

		public IEditCommandConfig InsertCommand { get; set; }

        [JsonIgnore]
        public SqlServerEditCommandConfig Insert => (SqlServerEditCommandConfig)InsertCommand;

        public IEditCommandConfig UpdateCommand { get; set; }
        [JsonIgnore]
        public SqlServerEditCommandConfig Update => (SqlServerEditCommandConfig)UpdateCommand;

        public IEditCommandConfig DeleteCommand { get; set; }
        [JsonIgnore]
        public SqlServerEditCommandConfig Delete => (SqlServerEditCommandConfig)DeleteCommand;

    }
}
