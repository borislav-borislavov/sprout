using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.Models.DataAdapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sprout.Core.SproutControlVMs
{
    public partial class SproutButtonVM : BaseSproutControlVM, IButtonActionHost, IDataAdapterHost
    {
        [ObservableProperty]
        private object _firstRow;

        public Dictionary<string, IButtonAction> ButtonActions { get; } = [];
        public IDataAdapter DataAdapter { get; set; }

        public SproutButtonVM(string name) : base(name)
        {
            
        }

        public void SetUpState()
        {

        }
    }
}
