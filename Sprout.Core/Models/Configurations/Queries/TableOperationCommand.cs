using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations.Queries
{
    public class TableOperationCommand
    {
        public string Text { get; set; }

        //the values used when inserting
        public Dictionary<string, string> DefaultValues { get; set; }
        //ex. UserStateID, 1
    }
}
