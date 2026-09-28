using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.WindowSize;
using Sprout.Core.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Sprout.Core.Windows
{
    /// <summary>
    /// Interaction logic for EditPage.xaml
    /// </summary>
    public partial class EditPage : Window
    {
        public bool HasSaved
        {
            get
            {
                if (_vm == null)
                    return false;

                return _vm.HasSaved;
            }
        }

        private EditPageVM _vm;

        public EditPage(EditPageVM vm)
        {
            InitializeComponent();

            WindowScreenSizer.SizeToScreen(this);

            _vm = vm;
            DataContext = vm;
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

        internal void InitializeVM(SproutPageConfiguration pageConfig)
        {
            if (DataContext is not EditPageVM vm)
                throw new Exception($"ViewModel should be {nameof(EditPageVM)}");

            vm.Initialize(pageConfig);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (_vm.ShouldClosePage() == false)
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}");
            }

            base.OnClosing(e);
        }
    }
}
