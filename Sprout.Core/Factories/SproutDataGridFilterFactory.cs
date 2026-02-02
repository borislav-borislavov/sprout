using Sprout.Core.Models.Configurations;
using Sprout.Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sprout.Core.Factories
{
    public static class SproutDataGridFilterFactory
    {
        internal static UIElement GetFilter(SqlServerFilterConfig filterConfig)
        {
            var filter = new SproutDataGridTextFilter();
            filter.GroupBox.Header = filterConfig.Title;

            return filter;
        }
    }
}
