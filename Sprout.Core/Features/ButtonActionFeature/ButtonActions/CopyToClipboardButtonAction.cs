using Sprout.Core.Factories;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Services.Clipboard;
using Sprout.Core.Models.Queries;

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

        public Task Perform(VMRegistry vmRegistry, IDataServiceFactory dataServiceFactory)
        {
            if (string.IsNullOrEmpty(_clipboardText))
                return Task.CompletedTask;

            _clipboardService.SetText(_clipboardText.ResolveDependencies(vmRegistry, false));

            return Task.CompletedTask;
        }
    }
}
