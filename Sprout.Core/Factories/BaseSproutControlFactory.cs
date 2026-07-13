using Sprout.Core.Models.Configurations;
using System.Windows;
using System.Windows.Controls;

namespace Sprout.Core.Factories
{
    public class BaseSproutControlFactory
    {
        protected static void SetPositionInGrid(FrameworkElement control, SproutControlConfig config)
        {
            Grid.SetRow(control, config.Row);
            Grid.SetColumn(control, config.Column);
            Grid.SetRowSpan(control, config.RowSpan);
            Grid.SetColumnSpan(control, config.ColumnSpan);
        }
    }
}
