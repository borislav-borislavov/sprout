using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public class SproutCheckBoxConfig : SproutControlConfig
    {
        public string Title { get; set; }

        public double? Height { get; set; }

        public double? Width { get; set; }

        public string Margin { get; set; }

        /// <summary>
        /// Binding expression using the syntax {@ctrlName.Property.Path}
        /// </summary>
        public string Binding { get; set; }

        public string HorizontalAlignment { get; set; }

        public string VerticalAlignment { get; set; }

        public string ToolTip { get; set; }
    }
}
