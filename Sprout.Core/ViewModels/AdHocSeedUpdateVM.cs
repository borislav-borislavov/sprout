using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Features.SeedFileUpdateFeature;

namespace Sprout.Core.ViewModels
{
    public partial class AdHocSeedUpdateVM : ObservableObject
    {
        [ObservableProperty]
        private string _filePath = string.Empty;

        public AdHocSeedUpdateVM(AdHocSeedUpdateConfig config)
        {
            FilePath = config?.FilePath ?? string.Empty;
        }

        public AdHocSeedUpdateConfig ToConfig()
        {
            return new AdHocSeedUpdateConfig
            {
                FilePath = FilePath
            };
        }
    }
}
