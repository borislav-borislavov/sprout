using System;
using System.Collections.Generic;
using System.Text;

namespace Sprout.Core.Services.Jobs
{
    public class Job : BaseSproutJob
    {
        public bool IsLiveDebug { get; set; }
        public Guid PageId { get; set; } = Guid.NewGuid();

        public override Task<string> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(string.Empty);
        }
    }
}
