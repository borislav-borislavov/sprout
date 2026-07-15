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
    public partial class SproutBorder : UserControl, ISproutControl<SproutBorderConfig, SproutBorderUIState>
    {
        public SproutBorderConfig Config { get; set; }
        public SproutControlType ControlType => SproutControlType.Border;
        public SproutBorderUIState VM { get; internal set; }

        public SproutBorder()
        {
            InitializeComponent();
        }
    }
}
