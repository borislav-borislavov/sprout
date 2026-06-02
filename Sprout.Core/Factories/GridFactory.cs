using Sprout.Core.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sprout.Core.Factories
{
    public class GridFactory : BaseSproutControlFactory
    {

        public static UIElement GenerateGrid(GridConfig gridConfig, Dictionary<string, UIElement> controls)
        {
            var grid = new Grid();
            
            AddControl(grid, controls);

            grid.ShowGridLines = gridConfig.ShowGridLines;

            if (!string.IsNullOrWhiteSpace(gridConfig.Background))
            {
                if (ColorConverter.ConvertFromString(gridConfig.Background) is Color color)
                {
                    grid.Background = new SolidColorBrush(color);
                }
            }

            if (!string.IsNullOrWhiteSpace(gridConfig.Margin))
            {
                if (new ThicknessConverter().ConvertFromString(gridConfig.Margin) is Thickness margin)
                {
                    grid.Margin = margin;
                }
            }

            if (gridConfig.Height.HasValue)
                grid.Height = gridConfig.Height.Value;

            if (gridConfig.Width.HasValue)
                grid.Width = gridConfig.Width.Value;

            if (!string.IsNullOrEmpty(gridConfig.HorizontalAlignment) &&
                gridConfig.HorizontalAlignment != "(Default)" &&
                Enum.TryParse<HorizontalAlignment>(gridConfig.HorizontalAlignment, out var hAlign))
            {
                grid.HorizontalAlignment = hAlign;
            }

            if (!string.IsNullOrEmpty(gridConfig.VerticalAlignment) &&
                gridConfig.VerticalAlignment != "(Default)" &&
                Enum.TryParse<VerticalAlignment>(gridConfig.VerticalAlignment, out var vAlign))
            {
                grid.VerticalAlignment = vAlign;
            }

            if (!string.IsNullOrEmpty(gridConfig.ToolTip))
                grid.ToolTip = gridConfig.ToolTip;

            foreach (var colSize in gridConfig.Columns)
            {
                var colDef = new ColumnDefinition { Width = ParseGridLength(colSize.Value) };
                grid.ColumnDefinitions.Add(colDef);
            }

            foreach (var rowSize in gridConfig.Rows)
            {
                var colDef = new RowDefinition { Height = ParseGridLength(rowSize.Value) };
                grid.RowDefinitions.Add(colDef);
            }

            foreach (var sproutControl in gridConfig.Children)
            {
                grid.Children.Add(SproutControlFactory.GetControl(sproutControl, controls));
            }

            SetPositionInGrid(grid, gridConfig);

            return grid;
        }

        private static System.Windows.GridLength ParseGridLength(string lengthString)
        {
            if (string.IsNullOrWhiteSpace(lengthString))
            {
                return new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);
            }

            lengthString = lengthString.Trim();

            if (lengthString.EndsWith("*"))
            {
                var numberPart = lengthString.Substring(0, lengthString.Length - 1);
                if (double.TryParse(numberPart, out var value))
                {
                    return new System.Windows.GridLength(value, System.Windows.GridUnitType.Star);
                }
                return new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);
            }

            if (lengthString.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                return new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto);
            }

            if (double.TryParse(lengthString, out var pixelValue))
            {
                return new System.Windows.GridLength(pixelValue, System.Windows.GridUnitType.Pixel);
            }

            return new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);
        }
    }
}
