using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public class SqlServerFilterConfig
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public EditorType EditorType { get; set; }

    }

    public enum EditorType
    {
        TextBox,
        ComboBox,
        CheckBox
    }
}
