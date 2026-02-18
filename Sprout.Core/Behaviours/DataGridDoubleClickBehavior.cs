using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sprout.Core.Behaviours
{
    public static class DataGridDoubleClickBehavior
    {
        public static readonly DependencyProperty DoubleClickCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "DoubleClickCommandParameter",
                typeof(object),
                typeof(DataGridDoubleClickBehavior),
                new PropertyMetadata(null));
        public static void SetDoubleClickCommandParameter(DependencyObject obj, object value)
            => obj.SetValue(DoubleClickCommandParameterProperty, value);

        public static object GetDoubleClickCommandParameter(DependencyObject obj)
            => obj.GetValue(DoubleClickCommandParameterProperty);

        public static readonly DependencyProperty DoubleClickProperty =
            DependencyProperty.RegisterAttached(
                "DoubleClickCommand",
                typeof(ICommand),
                typeof(DataGridDoubleClickBehavior),
                new PropertyMetadata(null, OnChanged));

        public static void SetDoubleClickCommand(DependencyObject obj, ICommand value)
            => obj.SetValue(DoubleClickProperty, value);

        public static ICommand GetDoubleClickCommand(DependencyObject obj)
            => (ICommand)obj.GetValue(DoubleClickProperty);

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid grid)
            {
                grid.MouseDoubleClick -= OnDoubleClick;
                grid.MouseDoubleClick += OnDoubleClick;
            }
        }

        private static void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid grid) return;

            var cmdParam = GetDoubleClickCommandParameter(grid);

            var command = GetDoubleClickCommand(grid);

            if (command?.CanExecute(cmdParam) == true)
                command.Execute(cmdParam);
        }
    }

}
