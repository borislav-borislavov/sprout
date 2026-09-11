using Sprout.Core.Features.SproutAppFeature;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sprout.Core.Features.SeedFileUpdateFeature
{
    public class SeedUpdaterFactory : ISeedUpdaterFactory
    {
        private readonly IConfigurationService _configurationService;
        private readonly IDialogService _dialogService;
        private readonly ISproutAppService _sproutAppService;

        public SeedUpdaterFactory(IConfigurationService configurationService, IDialogService dialogService, ISproutAppService sproutAppService)
        {
            _configurationService = configurationService;
            _dialogService = dialogService;
            _sproutAppService = sproutAppService;
        }

        public ISeedUpdater Create(ISeedUpdateConfig updateConfig)
        {
            if (updateConfig is AdHocSeedUpdateConfig adHocConfig)
            {
                return new AdHocSeedUpdater(_configurationService, adHocConfig, _dialogService, _sproutAppService);
            }

            throw new NotImplementedException($"Unsupported updater type '{updateConfig.GetType()}'");
        }
    }
}
