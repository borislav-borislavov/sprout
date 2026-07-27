using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.Services.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sprout.Core.SproutControlVMs
{
    public partial class BaseSproutControlVM: ObservableObject 
    {
        public string Name { get; set; }

        /// <summary>
        /// The ID of the page configuration that owns this control.
        /// </summary>
        public Guid OwnerPageID { get; set; }

        public BaseSproutControlVM(string name)
        {
            Name = name;
        }

        public virtual void SetUpState<T>(T control) where T : UserControl
        {
            throw new NotImplementedException();
        }
    }
}
