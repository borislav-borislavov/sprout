using Sprout.Core.Converters;
using Sprout.Core.Models.Configurations;
using Sprout.Core.UIStates;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Sprout.Core.Factories
{
    public class SproutListFactory : BaseSproutControlFactory, ISproutListFactory
    {
        public SproutList Create(SproutListConfig config,
            UIElement? itemTemplateRoot)
        {
            var sproutList = new SproutList
            {
                Name = config.Name,
                Config = config,
            };

            sproutList.Header = config.Header ?? string.Empty;
            sproutList.ShowSearch = config.ShowSearch;
            sproutList.ShowFooter = config.ShowFooter;
            sproutList.EmptyText = string.IsNullOrWhiteSpace(config.EmptyText)
                ? "No items to display"
                : config.EmptyText;

            if (config.ShowBorder)
            {
                if (!string.IsNullOrWhiteSpace(config.Background) &&
                    ColorConverter.ConvertFromString(config.Background) is Color background)
                {
                    sproutList.border.Background = new SolidColorBrush(background);
                }

                if (!string.IsNullOrWhiteSpace(config.BorderBrush) &&
                    ColorConverter.ConvertFromString(config.BorderBrush) is Color borderBrush)
                {
                    sproutList.border.BorderBrush = new SolidColorBrush(borderBrush);
                }

                sproutList.border.BorderThickness = new Thickness(config.BorderThickness);
                sproutList.border.CornerRadius = new CornerRadius(config.CornerRadius);
                sproutList.border.Padding = new Thickness(8);
            }
            else
            {
                sproutList.border.BorderThickness = new Thickness(0);
            }

            if (!string.IsNullOrWhiteSpace(config.Padding) &&
                new ThicknessConverter().ConvertFromString(config.Padding) is Thickness padding)
            {
                sproutList.border.Padding = padding;
            }

            if (config.Height.HasValue)
                sproutList.Height = config.Height.Value;

            if (config.Width.HasValue)
                sproutList.Width = config.Width.Value;

            if (!string.IsNullOrWhiteSpace(config.Margin))
            {
                if (new ThicknessConverter().ConvertFromString(config.Margin) is Thickness margin)
                    sproutList.Margin = margin;
            }

            if (!string.IsNullOrEmpty(config.HorizontalAlignment) &&
                config.HorizontalAlignment != "(Default)" &&
                Enum.TryParse<HorizontalAlignment>(config.HorizontalAlignment, out var hAlign))
            {
                sproutList.HorizontalAlignment = hAlign;
            }

            if (!string.IsNullOrEmpty(config.VerticalAlignment) &&
                config.VerticalAlignment != "(Default)" &&
                Enum.TryParse<VerticalAlignment>(config.VerticalAlignment, out var vAlign))
            {
                sproutList.VerticalAlignment = vAlign;
            }

            if (!string.IsNullOrEmpty(config.ToolTip))
                sproutList.ToolTip = config.ToolTip;

            sproutList.itemsControl.ItemTemplate = BuildItemTemplate(config.Child, itemTemplateRoot);

            SetPositionInGrid(sproutList, config);

            var uiState = new SproutListUIState(sproutList.Name);
            uiState.SetUpState(sproutList);

            return sproutList;
        }

        /// <summary>
        /// Builds the per-item <see cref="DataTemplate"/> from the configured child control.
        /// The child is rendered by the dispatcher and cloned into a reusable template.
        /// Falls back to a simple data-summary template when no usable child is configured or
        /// when the child cannot be cloned.
        /// </summary>
        private static DataTemplate BuildItemTemplate(SproutControlConfig childConfig,
            UIElement? childElement)
        {
            // An empty grid (the default child) carries no visuals, so render the row data instead.
            if (childConfig is null ||
                (childConfig is GridConfig grid && grid.Children.Count == 0))
            {
                return BuildFallbackTemplate();
            }

            try
            {
                if (childElement is null)
                {
                    return BuildFallbackTemplate();
                }

                var childXaml = XamlWriter.Save(childElement);

                var templateXaml =
                    "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                    "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                    childXaml +
                    "</DataTemplate>";

                if (XamlReader.Parse(templateXaml) is DataTemplate template)
                {
                    return template;
                }
            }
            catch
            {
                // Cloning can fail for controls relying on code-behind/named bindings; fall back safely.
            }

            return BuildFallbackTemplate();
        }

        private static DataTemplate BuildFallbackTemplate()
        {
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(0xEC, 0xF0, 0xF1)));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 0, 0, 1));
            border.SetValue(Border.PaddingProperty, new Thickness(4, 6, 4, 6));

            var text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            text.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(0x2C, 0x3E, 0x50)));
            text.SetBinding(TextBlock.TextProperty, new Binding
            {
                Converter = new DataRowViewToStringConverter()
            });

            border.AppendChild(text);

            return new DataTemplate { VisualTree = border };
        }
    }
}
