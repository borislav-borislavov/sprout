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

        public SproutControlConfig Root { get; set; }

        public List<QueryConfig> Queries { get; set; } = [];
    }

    public class QueryConfig
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string TableName { get; set; }

        //it sounds like a cool idea to be able to pull data from a different database per query
        public string ConnectionString { get; set; }

        public TableOperationCommand InsertCommand { get; set; }
        public TableOperationCommand UpdateCommand { get; set; }
        public TableOperationCommand DeleteCommand { get; set; }
    }

    public class TableOperationCommand
    {
        public string Text { get; set; }

        //the values used when inserting
        public Dictionary<string, string> DefaultValues { get; set; }
        //ex. UserStateID, 1
    }

    public abstract class SproutControlConfig
    {
        public int Row { get; set; }
        public int RowSpan { get; set; }

        public int Column { get; set; }
        public int ColumnSpan { get; set; }
    }

    public class GridConfig : SproutControlConfig
    {
        public List<string> Rows { get; set; } = [];

        public List<string> Columns { get; set; } = [];

        public List<SproutControlConfig> Children { get; set; } = [];
    }

    public class ButtonConfig : SproutControlConfig
    {
        public string Content { get; set; }
    }

    public class  SproutDataGridConfig : SproutControlConfig
    {
        public string QueryName { get; set; }
        public string Name { get; internal set; }

        public bool AllowInsert { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }

        public bool ShowSave => AllowInsert || AllowUpdate || AllowDelete;

        public List<SproutDataGridColumnConfig> Columns { get; set; }
    }

    public class SproutDataGridColumnConfig
    {
        public string Header { get; set; }
        public string BindingPath { get; set; }
    }

    //public class GridActionConfig
    //{
    //    public SproutDataGridConfig Parent { get; set; }

    //    public string Text { get; set; }
    //    public string IconGlyph { get; set; }
    //    public string ToolTip { get; set; }
    //    public Type MyProperty { get; set; }
    //}
}
