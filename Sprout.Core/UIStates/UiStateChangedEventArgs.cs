using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.UIStates
{
    public sealed record UiStateChangedEventArgs
    {
        public string ControlName { get; set; }
        public string PropertyName { get; set; }

        public UiStateChangedEventArgs(object control, string propertyName)
        {
            ControlName = (control as BaseUIState).Name;
            PropertyName = propertyName;
        }
    }
}
