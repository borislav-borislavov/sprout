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
#nullable disable

namespace Sprout.Core.Views.Controls
{
    /// <summary>
    /// Interaction logic for SproutCombo.xaml
    /// </summary>
    public partial class SproutCombo : UserControl
    {
        internal SproutComboConfig Config { get; set; }
        public SproutComboUIState UIState { get; internal set; }

        public SproutCombo()
        {
            InitializeComponent();
        }
    }
}
