using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.UIStates;
using System.Windows.Controls;

namespace Sprout.Core.Views.Controls
{
    /// <summary>
    /// Interaction logic for SproutDatePicker.xaml
    /// </summary>
    public partial class SproutDatePicker : UserControl, ISproutControl<SproutDatePickerConfig>
    {
        public SproutDatePickerConfig Config { get; set; }
        public SproutControlType ControlType => SproutControlType.DatePicker;
        public SproutDatePickerUIState UIState { get; internal set; }

        public SproutDatePicker()
        {
            InitializeComponent();
        }
    }
}
