using CommunityToolkit.Mvvm.Messaging;
using Sprout.Core.Factories;
using Sprout.Core.Messages;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.SproutControlVMs;
using System;
using System.Threading.Tasks;

namespace Sprout.Core.Features.ButtonActions.Actions
{
    public class OpenPageButtonAction : IButtonAction
    {
        private readonly Guid _pageID;
        private readonly bool _openAsDialog;

        public OpenPageButtonAction(Guid pageID, bool openAsDialog)
        {
            _pageID = pageID;
            _openAsDialog = openAsDialog;
        }

        public Task Perform(VMRegistry vmRegistry, IDataServiceFactory dataServiceFactory)
        {
            var args = new OpenTabMessageArgs()
            {
                PageConfigID = _pageID,
                OpenAsDialog = _openAsDialog
            };

            WeakReferenceMessenger.Default.Send(new OpenTabMessage(args));

            return Task.CompletedTask;
        }
    }
}
