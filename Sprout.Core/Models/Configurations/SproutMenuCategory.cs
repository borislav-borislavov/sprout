namespace Sprout.Core.Models.Configurations
{
    public class SproutMenuCategory
    {
        public const string DefaultBackgroundColor = "#FFEFEFEF";

        public Guid ID { get; set; }

        public string Title { get; set; }

        public string BackgroundColor { get; set; } = DefaultBackgroundColor;
    }
}
