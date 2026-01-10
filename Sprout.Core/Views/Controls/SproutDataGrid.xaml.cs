using Microsoft.VisualBasic;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.Queries;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for SproutDataGrid.xaml
    /// </summary>
    public partial class SproutDataGrid : UserControl
    {
        public SproutDataGridConfig Config { get; set; }

        public SproutGridUIState UIState { get; set; }

        public SproutDataGrid()
        {
            InitializeComponent();

        }
    }
}
