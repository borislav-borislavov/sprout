using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations;
using System;
using System.Collections.ObjectModel;

namespace Sprout.Core.ViewModels
{
    public partial class MenuCategoryVM : ObservableObject
    {
        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private string _backgroundColor = SproutMenuCategory.DefaultBackgroundColor;

        public Guid ID { get; set; } = Guid.NewGuid();

        public ObservableCollection<SproutPageConfiguration> Pages { get; } = [];

        [RelayCommand]
        private void ToggleExpanded()
        {
            IsExpanded = !IsExpanded;
        }
    }
}
