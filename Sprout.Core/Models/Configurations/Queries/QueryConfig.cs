using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations.Queries
{
    public class QueryConfig : IDataProviderConfig
    {
        [Obsolete("Because every DataProvider will belong to a control the control name could be used instead")]
        public string ProviderName { get; set; }
        public string Text { get; set; }
        public string TableName { get; set; }

        //it sounds like a cool idea to be able to pull data from a different database per query
        public string ConnectionString { get; set; }

        public TableOperationCommand InsertCommand { get; set; }
        public TableOperationCommand UpdateCommand { get; set; }
        public TableOperationCommand DeleteCommand { get; set; }
    }
}
