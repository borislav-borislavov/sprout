using CommunityToolkit.Mvvm.ComponentModel;

namespace Sprout.Core.SproutControlVMs
{
    public partial class FilterPresetItem : ObservableObject
    {
        public FilterPresetItem(string name, bool isNone = false)
        {
            Name = name;
            IsNone = isNone;
        }

        public string Name { get; }

        public bool IsNone { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayName))]
        private bool _isDefault;

        public string DisplayName => IsDefault ? $"★ {Name}" : Name;
    }
}
