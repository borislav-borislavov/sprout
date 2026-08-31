using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Models.Configurations;
using System.Collections.ObjectModel;

namespace Sprout.Core.ViewModels
{
    public partial class SeedPickerVM : ObservableObject
    {
        public ObservableCollection<SeedFile> Seeds { get; } = [];

        [ObservableProperty]
        private SeedFile? _selectedSeed;

        public void LoadSeeds(IEnumerable<SeedFile> seeds)
        {
            Seeds.Clear();
            foreach (var seed in seeds)
            {
                Seeds.Add(seed);
            }
        }
    }
}
