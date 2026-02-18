using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public class SproutTextBoxConfig : SproutControlConfig
    {
        public string Placeholder { get; set; }

        public string Title { get; set; }

        public double? Height { get; set; }

        public double? Width { get; set; }

        /// <summary>
        /// Binding expression using the syntax {@ctrlName.Property.Path}
        /// </summary>
        public string Binding { get; set; }
    }
}
