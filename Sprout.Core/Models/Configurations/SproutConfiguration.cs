using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sprout.Core.Models.Configurations
{
    public class SproutConfiguration
    {
        public IEnumerable<SproutPageConfiguration> Pages { get; set; }
    }

    public class SproutPageConfiguration
    {
        public string Title { get; set; }

        public SproutConfig Root { get; set; }
    }

    public abstract class SproutConfig
    {
        public int Row { get; set; }
        public int RowSpan { get; set; }

        public int Column { get; set; }
        public int ColumnSpan { get; set; }
    }

    public class GridConfig : SproutConfig
    {
        public List<string> Rows { get; set; } = [];

        public List<string> Columns { get; set; } = [];

        public List<SproutConfig> Children { get; set; } = [];
    }

    public class ButtonConfig : SproutConfig
    {
        public string Content { get; set; }
    }
}
