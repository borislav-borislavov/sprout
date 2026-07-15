using Sprout.Core.Factories;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Services.Clipboard;
using Sprout.Core.Features.Dependency;
using Sprout.Core.Models.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sprout.Core.Features.ButtonActions;

namespace Sprout.Core.Features.ButtonActions.Actions
{
    public class CopyToClipboardButtonAction : IButtonAction
    {
        private readonly string _clipboardText;
        private readonly IClipboardService _clipboardService;

        public CopyToClipboardButtonAction(string clipboardText, IClipboardService clipboardService)
        {
            _clipboardText = clipboardText;
            _clipboardService = clipboardService;
        }

        public Task Perform(Dictionary<string, IDataAdapter> dataAdapters, VMRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
        {
            if (string.IsNullOrEmpty(_clipboardText))
                return Task.CompletedTask;

            string textToCopy = _clipboardText;

            var dep = DependencyParser.ParseDependencies(_clipboardText).FirstOrDefault();
            if (dep != null)
            {
                var uiState = uiStateRegistry.Get(dep.ControlName);

                if (uiState == null)
                {
                    return Task.CompletedTask;
                }

                var baseUiState = uiState as BaseSproutControlVM;

                var val = BindingEvaluator.Evaluate(uiState, dep.PropertyPath);
                textToCopy = val?.ToString() ?? string.Empty;
            }

            _clipboardService.SetText(textToCopy);

            return Task.CompletedTask;
        }
    }
}
