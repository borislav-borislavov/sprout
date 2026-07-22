using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Views;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sprout.Core.SproutControlVMs
{
    public partial class SproutComboVM : BaseSproutControlVM, IDataAdapterHost
    {
        [ObservableProperty]
        private object _selected;

        [ObservableProperty]
        private IDataAdapter _dataAdapter;

        public SproutComboVM(string name) : base(name)
        {
            
        }

        public virtual void SetUpState(SproutCombo control)
        {
            control.VM = this;

            control.comboBox.SetBinding(DataGrid.SelectedItemProperty,
                new Binding(nameof(this.Selected))
                {
                    Source = this,
                    Mode = BindingMode.TwoWay
                });
        }
    }
}
