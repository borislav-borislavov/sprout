using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    /// <summary>
    /// UI Editor for this config is <see cref="Sprout.Core.UIEditors.EditSproutTextBox"/>
    /// </summary>
    public class SproutTextBoxConfig : SproutControlConfig
    {
        public string Placeholder { get; set; }

        public string Title { get; set; }

        public double? Height { get; set; }

        public double? Width { get; set; }

        public string Margin { get; set; }

        /// <summary>
        /// Binding expression using the syntax {@ctrlName.Property.Path}
        /// </summary>
        public string Binding { get; set; }

        public bool MultiLine { get; set; }

        public string HorizontalAlignment { get; set; }

        public string VerticalAlignment { get; set; }

        public string ToolTip { get; set; }
        public bool ChangeValueOnEnter { get; set; }

        /// <summary>
        /// When true, the text box accepts dropped files and sets its text to the
        /// full path of the dropped file.
        /// </summary>
        public bool AllowFileDrop { get; set; }
    }
}
