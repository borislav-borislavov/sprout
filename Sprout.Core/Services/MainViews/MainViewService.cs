using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sprout.Core.Services.MainViews
{
    public class MainViewService : IMainViewService
    {
        public MainView MainView { get; set; }

        public MainViewService()
        {
            MainView = new MainView();
        }
    }
}
