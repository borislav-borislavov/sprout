using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.SproutControlVMs;
using System.Windows.Controls;
#nullable disable

namespace Sprout.Core.Views.Controls
{
    /// <summary>
    /// Interaction logic for SproutBorder.xaml
    /// </summary>
    public partial class SproutBorder : UserControl, ISproutControl<SproutBorderConfig, SproutBorderVM>
    {
        public SproutBorderConfig Config { get; set; }
        public SproutControlType ControlType => SproutControlType.Border;
        public SproutBorderVM VM { get; internal set; }

        public SproutBorder()
        {
            InitializeComponent();
        }
    }
}
