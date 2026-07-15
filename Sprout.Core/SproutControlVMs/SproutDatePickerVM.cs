using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Views.Controls;
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sprout.Core.SproutControlVMs
{
    public partial class SproutDatePickerVM : BaseSproutControlVM
    {
        [ObservableProperty]
        private DateTime? _selectedDate;

        [ObservableProperty]
        private string? _date;

        private string _outputDateFormat = "yyyy-MM-dd";

        public SproutDatePickerVM(string name) : base(name)
        {
            
        }

        partial void OnSelectedDateChanged(DateTime? value)
        {
            Date = value?.ToString(_outputDateFormat);
        }

        public virtual void SetUpState(SproutDatePicker control)
        {
            control.VM = this;
            _outputDateFormat = control.Config.OutputDateFormat ?? "yyyy-MM-dd";

            control.datePicker.SetBinding(DatePicker.SelectedDateProperty,
                new Binding(nameof(this.SelectedDate))
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
        }
    }
}
