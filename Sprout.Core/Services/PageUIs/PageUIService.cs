using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Sprout.Core.Services.PageUIs
{
    public class PageUIService : IPageUIService
    {
        public UIElement CreateUI(SproutPageConfiguration pageConfig)
        {
            var root = pageConfig.Root;

            return SproutControlFactory.GetControl(pageConfig.Root);
        }
    }
}
