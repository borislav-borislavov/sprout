using Sprout.Core.Models.Configurations.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public class SproutPageConfiguration
    {
        public string Title { get; set; }

        public SproutControlConfig Root { get; set; }

        public List<QueryConfig> Queries { get; set; } = [];
    }
}
