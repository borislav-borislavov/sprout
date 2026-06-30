using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    /// <summary>
    /// UI Editor for this config is <see cref="Sprout.Core.Views.EditSproutLabel"/>.
    /// A display-only control that renders text. Does not host a DataAdapter.
    /// </summary>
    public class SproutLabelConfig : SproutControlConfig
    {
        /// <summary>
        /// Static text shown by the label. Used as the initial value and as a
        /// fallback when <see cref="Binding"/> is not set.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Binding expression using the syntax {@ctrlName.Property.Path}.
        /// When set, the displayed text reactively follows the referenced control's state.
        /// </summary>
        public string Binding { get; set; }

        /// <summary>
        /// Text color. Accepts hex codes (#FF0000) or named colors (Red).
        /// </summary>
        public string Foreground { get; set; }

        /// <summary>
        /// Font family name, for example "Segoe UI".
        /// </summary>
        public string FontFamily { get; set; }

        public double? FontSize { get; set; }

        /// <summary>
        /// Font weight name, for example "Bold" or "Normal".
        /// </summary>
        public string FontWeight { get; set; }

        /// <summary>
        /// Font style name, for example "Italic" or "Normal".
        /// </summary>
        public string FontStyle { get; set; }

        public bool TextWrapping { get; set; }

        public double? Height { get; set; }

        public double? Width { get; set; }

        public string Margin { get; set; }

        public string HorizontalAlignment { get; set; }

        public string VerticalAlignment { get; set; }

        public string ToolTip { get; set; }
    }
}
