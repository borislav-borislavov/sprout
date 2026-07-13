using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sprout.Core.Views.Controls
{
    /// <summary>
    /// Interaction logic for SproutLabel.xaml
    /// </summary>
    public partial class SproutLabel : UserControl, ISproutControl<SproutLabelConfig, SproutLabelUIState>
    {
        public SproutLabelConfig Config { get; set; }
        public SproutControlType ControlType => SproutControlType.Label;
        public SproutLabelUIState VM { get; internal set; }

        public SproutLabel()
        {
            InitializeComponent();
        }
    }
}
