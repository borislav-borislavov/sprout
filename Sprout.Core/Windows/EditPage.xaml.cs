using Sprout.Core.Models.Configurations;
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
using System.Windows.Shapes;

namespace Sprout.Core.Windows
{
    /// <summary>
    /// Interaction logic for EditPage.xaml
    /// </summary>
    public partial class EditPage : Window
    {
        public EditPage()
        {
            InitializeComponent();

            this.Width = SystemParameters.PrimaryScreenWidth * 0.85;
            this.Height = SystemParameters.PrimaryScreenHeight * 0.85;

            // Optional: Center the window on the screen
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void ControlsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ((EditPageVM)DataContext).SelectedNode = (SproutControlConfig)e.NewValue;
        }

        private void ControlsTree_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (DataContext is not EditPageVM vm
                || vm.SelectedNode is not SproutControlConfig config)
            {
                e.Handled = true;
                return;
            }

            vm.PrepareMove(config);

            MoveToMenuItem.Items.Clear();

            if (vm.MoveParentOptions.Count == 0)
            {
                e.Handled = true;
                return;
            }

            foreach (var parentOption in vm.MoveParentOptions)
            {
                var menuItem = new MenuItem
                {
                    Header = parentOption.Name,
                    Command = vm.MoveToParentCommand,
                    CommandParameter = parentOption
                };
                MoveToMenuItem.Items.Add(menuItem);
            }
        }
    }
}
