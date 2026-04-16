using Sprout.Core.Models.Configurations;
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
    /// Interaction logic for SproutButton.xaml
    /// </summary>
    public partial class SproutButton : UserControl
    {
        internal SproutButtonConfig Config { get; set; }

        public SproutButton()
        {
            InitializeComponent();
        }
    }
}
