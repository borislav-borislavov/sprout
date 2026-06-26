using Sprout.Core.Models.Configurations;
using Sprout.Core.UIStates;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Sprout.Core.Factories
{
    public class SproutDatePickerFactory : BaseSproutControlFactory
    {
        public static SproutDatePicker Create(SproutDatePickerConfig config,
            Dictionary<string, UIElement> controls)
        {
            var sproutDatePicker = new SproutDatePicker
            {
                Name = config.Name,
                Config = config,
            };

            if (config.Height.HasValue)
            {
                sproutDatePicker.datePicker.Height = config.Height.Value;
            }

            if (config.Width.HasValue)
            {
                sproutDatePicker.datePicker.Width = config.Width.Value;
            }

            if (!string.IsNullOrEmpty(config.Label))
            {
                sproutDatePicker.lblTitle.Text = config.Label;
                sproutDatePicker.lblTitle.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrWhiteSpace(config.Margin))
            {
                if (new ThicknessConverter().ConvertFromString(config.Margin) is Thickness margin)
                {
                    sproutDatePicker.Margin = margin;
                }
            }

            if (!string.IsNullOrEmpty(config.HorizontalAlignment) &&
                config.HorizontalAlignment != "(Default)" &&
                Enum.TryParse<HorizontalAlignment>(config.HorizontalAlignment, out var hAlign))
            {
                sproutDatePicker.HorizontalAlignment = hAlign;
            }

            if (!string.IsNullOrEmpty(config.VerticalAlignment) &&
                config.VerticalAlignment != "(Default)" &&
                Enum.TryParse<VerticalAlignment>(config.VerticalAlignment, out var vAlign))
            {
                sproutDatePicker.VerticalAlignment = vAlign;
            }

            if (!string.IsNullOrEmpty(config.ToolTip))
            {
                sproutDatePicker.ToolTip = config.ToolTip;
            }

            AddControl(sproutDatePicker, controls);
            SetPositionInGrid(sproutDatePicker, config);

            var uiState = new SproutDatePickerUIState();
            uiState.SetUpState(sproutDatePicker);

            // Set the initial date via UIState so it flows through the binding
            uiState.SelectedDate = ComputeInitialDate(config);

            return sproutDatePicker;
        }

        private static DateTime ComputeInitialDate(SproutDatePickerConfig config)
        {
            DateTime baseDate;

            switch (config.DateMode)
            {
                case SproutDatePickerMode.CurrentDate:
                    baseDate = DateTime.Today;
                    break;
                case SproutDatePickerMode.CustomDate:
                    baseDate = config.CustomDate ?? DateTime.Today;
                    break;
                case SproutDatePickerMode.StartOfMonth:
                    baseDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    break;
                case SproutDatePickerMode.EndOfMonth:
                    var today = DateTime.Today;
                    baseDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                    break;
                default:
                    baseDate = DateTime.Today;
                    break;
            }

            // Apply month offset first, then day offset
            if (config.MonthOffset != 0)
            {
                baseDate = baseDate.AddMonths(config.MonthOffset);
            }

            if (config.DayOffset != 0)
            {
                baseDate = baseDate.AddDays(config.DayOffset);
            }

            return baseDate;
        }
    }
}
