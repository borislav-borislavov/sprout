using Sprout.Core.Models.Configurations;
using Sprout.Core.Services;
using Sprout.Core.Services.PageUIs;
using Sprout.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sprout.Core.Views
{
    /// <summary>
    /// Interaction logic for SproutPage.xaml
    /// </summary>
    public partial class SproutPage : UserControl
    {
        private IPageUIService _pageUIService;

        public SproutPage(IPageUIService pageUIService, SproutPageConfiguration pageConfig)
        {
            _pageUIService = pageUIService;
            InitializeComponent();
            DataContext = new SproutPageVM(pageConfig);

            this.Content = _pageUIService.CreateUI(pageConfig);
        }
    }
}
