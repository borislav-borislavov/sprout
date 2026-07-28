using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public partial class SproutControlConfig : ObservableObject
    {
        public string Name { get; set; }

        [ObservableProperty]
        private int _row;

        public int RowSpan { get; set; } = 1;

        [ObservableProperty]
        private int _column;

        public int ColumnSpan { get; set; } = 1;
    }
}
