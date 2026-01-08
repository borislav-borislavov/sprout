using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Messages
{
    public sealed class CloseWindowMessage : ValueChangedMessage<string>
    {
        public CloseWindowMessage()
            : base(string.Empty)
        {

        }
    }
}
