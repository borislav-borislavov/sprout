using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sprout.Core.Features.SeedFileUpdateFeature
{
    public class AdHocSeedUpdateConfig : ISeedUpdateConfig
    {
        public string FilePath { get; set; } = string.Empty;
    }
}
