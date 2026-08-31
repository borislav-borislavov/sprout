using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

namespace Sprout.Core.Models.DataAdapters.Filters
{
    public partial class SqlServerFilter : ObservableObject, IFilter
    {
        public string Title { get; set; }

        public string Text { get; set; }

        [ObservableProperty]
        private object _startValue;

        [ObservableProperty]
        private object _endValue;

        public bool IsRange { get; set; }
        public string DefaultValue { get; set; }
    }
}
