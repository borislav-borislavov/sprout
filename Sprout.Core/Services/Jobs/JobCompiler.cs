using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.CPL;
using System.Text;

namespace Sprout.Core.Services.Jobs
{
    public sealed class JobCompiler : BaseCompiler
    {
        private static readonly string[] _baseUsings =
        [
            "System",
            "System.Collections.Generic",
            "System.IO",
            "System.Linq",
            "System.Threading",
            "System.Threading.Tasks",
            "Sprout.Core.Services.Jobs",
            "ClosedXML.Excel"
        ];

        private readonly SproutJobConfiguration _job;
        private readonly IConfigurationService _configurationService;

        protected override string[] Usings =>
            _baseUsings
                .Concat(_job.Usings ?? [])
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

        public JobCompiler(SproutJobConfiguration job, IConfigurationService configurationService)
            : base(job.ID.ToString("N"))
        {
            _job = job;
            _configurationService = configurationService;
            UserScript = job.Script;
        }

        public override IEnumerable<string> GetCompletionHints() =>
            ["ExecuteAsync", "cancellationToken"];

        public override IReadOnlyList<string> GetAdditionalUsings() => _job.Usings ?? [];

        public override void ApplyAdditionalUsings(IEnumerable<string> usings)
        {
            _job.Usings = usings
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select(u => u.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
            InvalidateTypeIndex();
        }

        public override string GetSource()
        {
            return $$"""
                {{BuildUsings()}}

                namespace DynamicJob._{{PageId}}
                {
                    public sealed class Job : BaseSproutJob
                    {
                        public bool IsLiveDebug { get; set; } = true;
                        public Guid PageId { get; set; } = Guid.Parse("{{PageId}}");

                #line 1 "JobScript"
                {{UserScript}}
                #line default
                    }
                }
                """;
        }

        public override void SaveUserScript()
        {
            _job.Script = UserScript;
            var config = _configurationService.Load();
            var index = config.Jobs.FindIndex(j => j.ID == _job.ID);
            if (index < 0)
                throw new InvalidOperationException($"Job '{_job.Name}' no longer exists.");

            config.Jobs[index] = _job;
            if (!_configurationService.Save(config))
                throw new InvalidOperationException("The job script could not be saved.");
        }
    }
}
