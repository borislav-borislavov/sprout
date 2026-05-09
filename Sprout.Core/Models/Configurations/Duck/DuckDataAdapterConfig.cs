using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations.Duck
{
    public class DuckDataAdapterConfig : IDataAdapterConfig
    {
        public string ConnectionString { get; set; }

        public IDataProviderConfig DataProvider { get; set; }

        public IEditCommandConfig InsertCommand { get; set; }

        [JsonIgnore]
        public DuckEditCommandConfig Insert => (DuckEditCommandConfig)InsertCommand;

        public IEditCommandConfig UpdateCommand { get; set; }
        [JsonIgnore]
        public DuckEditCommandConfig Update => (DuckEditCommandConfig)UpdateCommand;

        public IEditCommandConfig DeleteCommand { get; set; }
        [JsonIgnore]
        public DuckEditCommandConfig Delete => (DuckEditCommandConfig)DeleteCommand;
    }
}