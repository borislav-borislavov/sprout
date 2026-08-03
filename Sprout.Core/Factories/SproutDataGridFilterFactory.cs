using Sprout.Core.Models.Configurations;
using Sprout.Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Sprout.Core.Factories
{
    public static class SproutDataGridFilterFactory
    {
        internal static UIElement GetFilter(FilterConfig filterConfig)
        {
            if (filterConfig.EditorType == EditorType.TextBox)
            {
                var filter = new SproutDataGridTextFilter();
                filter.GroupBox.Header = filterConfig.Title;
                return filter;
            }
            else if (filterConfig.EditorType == EditorType.CheckBox)
            {
                var filter = new SproutDataGridCheckBoxFilter();
                filter.GroupBox.Header = filterConfig.Title;
                return filter;
            }

            throw new NotImplementedException($"Filter of type {filterConfig.EditorType} is not implemented.");
        }
    }
}
