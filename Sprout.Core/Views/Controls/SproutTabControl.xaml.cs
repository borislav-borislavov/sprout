using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.SproutControlVMs;
using System.Windows.Controls;
#nullable disable

namespace Sprout.Core.Views.Controls
{
    /// <summary>
    /// Interaction logic for SproutTabControl.xaml
    /// </summary>
    public partial class SproutTabControl : UserControl, ISproutControl<SproutTabControlConfig, SproutTabControlVM>
    {
        public SproutTabControlConfig Config { get; set; }
        public SproutControlType ControlType => SproutControlType.TabControl;
        public SproutTabControlVM VM { get; internal set; }

        public SproutTabControl()
        {
            InitializeComponent();
        }
    }
}
