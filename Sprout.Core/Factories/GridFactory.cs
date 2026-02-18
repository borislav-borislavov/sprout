using Sprout.Core.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Sprout.Core.Factories
{
    public class GridFactory : BaseSproutControlFactory
    {

        public static UIElement GenerateGrid(GridConfig gridConfig, Dictionary<string, UIElement> controls)
        {
            var grid = new Grid();
            
            AddControl(grid, controls);

            grid.ShowGridLines = gridConfig.ShowGridLines;

            foreach (var colSize in gridConfig.Columns)
            {
                var colDef = new ColumnDefinition { Width = ParseGridLength(colSize) };
                grid.ColumnDefinitions.Add(colDef);
            }

            foreach (var rowSize in gridConfig.Rows)
            {
                var colDef = new RowDefinition { Height = ParseGridLength(rowSize) };
                grid.RowDefinitions.Add(colDef);
            }

            foreach (var sproutControl in gridConfig.Children)
            {
                grid.Children.Add(SproutControlFactory.GetControl(sproutControl, controls));
            }

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
