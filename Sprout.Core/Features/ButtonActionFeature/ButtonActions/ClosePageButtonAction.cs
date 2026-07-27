using CommunityToolkit.Mvvm.Messaging;
using Sprout.Core.Factories;
using Sprout.Core.Messages;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.SproutControlVMs;
using System;
using System.Threading.Tasks;

namespace Sprout.Core.Features.ButtonActions.Actions
{
    public class ClosePageButtonAction : IButtonAction
    {
        private readonly Guid _pageID;

        public ClosePageButtonAction(Guid pageID)
        {
            _pageID = pageID;
        }

        public Task Perform(VMRegistry vmRegistry, IDataServiceFactory dataServiceFactory)
        {
            var args = new CloseTabMessageArgs()
            {
                PageConfigID = _pageID
            };

            WeakReferenceMessenger.Default.Send(new CloseTabMessage(args));

            return Task.CompletedTask;
        }
    }
}
