using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sprout.Core.ViewModels
{
    internal class SproutPageVM : ObservableObject
    {
        private SproutPageConfiguration _pageConfig;

        public SproutPageVM(SproutPageConfiguration pageConfig)
        {
            this._pageConfig = pageConfig;
        }
    }
}
