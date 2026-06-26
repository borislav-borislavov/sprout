using System;

namespace Sprout.Core.Models.Configurations
{
    public class SproutDatePickerConfig : SproutControlConfig
    {
        public string Label { get; set; }

        public SproutDatePickerMode DateMode { get; set; } = SproutDatePickerMode.CurrentDate;

        public DateTime? CustomDate { get; set; }

        public int DayOffset { get; set; }

        public int MonthOffset { get; set; }

        public string OutputDateFormat { get; set; } = "yyyy-MM-dd";

        public double? Height { get; set; }

        public double? Width { get; set; }

        public string Margin { get; set; }

        public string HorizontalAlignment { get; set; }

        public string VerticalAlignment { get; set; }

        public string ToolTip { get; set; }
    }
}
