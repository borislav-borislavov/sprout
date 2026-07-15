using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Features.ButtonActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sprout.Core.UIStates
{
    public partial class SproutButtonVM : BaseSproutControlVM, IButtonActionHost
    {
        [ObservableProperty]
        private object _firstRow;

        public Dictionary<string, IButtonAction> ButtonActions { get; } = [];

        public SproutButtonVM(string name) : base(name)
        {
            
        }

        public void SetUpState()
        {

        }
    }
}
