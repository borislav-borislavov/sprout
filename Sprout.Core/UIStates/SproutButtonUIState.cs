using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sprout.Core.UIStates
{
    public partial class SproutButtonUIState : BaseUIState
    {
        [ObservableProperty]
        private object _firstRow;

        public void SetUpState(string controlName)
        {
            this.Name = controlName;
        }
    }
}
