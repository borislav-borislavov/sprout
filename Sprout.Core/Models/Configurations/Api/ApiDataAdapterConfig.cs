using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations.Api
{
    public class ApiDataAdapterConfig : IDataAdapterConfig
    {
        public string Name { get; set; }
        public Type ParentType { get; set; }

        public IDataProviderConfig DataProvider { get; set; }

        public IEditCommandConfig InsertCommand { get; set; }

        [JsonIgnore]
        public ApiEditCommandConfig Insert => (ApiEditCommandConfig)InsertCommand;

        public IEditCommandConfig UpdateCommand { get; set; }

        [JsonIgnore]
        public ApiEditCommandConfig Update => (ApiEditCommandConfig)UpdateCommand;

        public IEditCommandConfig DeleteCommand { get; set; }

        [JsonIgnore]
        public ApiEditCommandConfig Delete => (ApiEditCommandConfig)DeleteCommand;
    }
}
