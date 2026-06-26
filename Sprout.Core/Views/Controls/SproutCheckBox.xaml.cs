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
    /// Interaction logic for SproutCheckBox.xaml
    /// </summary>
    public partial class SproutCheckBox : UserControl, ISproutControl<SproutCheckBoxConfig>
    {
        public SproutCheckBoxConfig Config { get; set; }
        public SproutControlType ControlType => SproutControlType.CheckBox;
        public SproutCheckBoxUIState UIState { get; internal set; }

        public SproutCheckBox()
        {
            InitializeComponent();
        }
    }
}
