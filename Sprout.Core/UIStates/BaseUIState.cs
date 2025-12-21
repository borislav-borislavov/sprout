using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sprout.Core.UIStates
{
    public partial class BaseUIState: ObservableObject 
    {
        public string Name { get; set; }

        public virtual void SetUpState<T>(T control) where T : UserControl
        {
            throw new NotImplementedException();
        }
    }
}
