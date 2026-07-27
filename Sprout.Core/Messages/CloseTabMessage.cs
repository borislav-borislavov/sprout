using CommunityToolkit.Mvvm.Messaging.Messages;
using System;

namespace Sprout.Core.Messages
{
    public class CloseTabMessage : ValueChangedMessage<CloseTabMessageArgs>
    {
        public CloseTabMessage(CloseTabMessageArgs value)
            : base(value)
        {

        }
    }

    public class CloseTabMessageArgs
    {
        public Guid PageConfigID { get; set; }
    }
}
