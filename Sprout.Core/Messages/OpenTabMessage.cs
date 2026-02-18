using CommunityToolkit.Mvvm.Messaging.Messages;
using Sprout.Core.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Messages
{
    internal class OpenTabMessage : ValueChangedMessage<OpenTabMessageArgs>
    {
        public OpenTabMessage(OpenTabMessageArgs value) 
            : base(value)
        {

        }
    }

    internal class OpenTabMessageArgs
    {
        public Guid PageConfigID { get; set; }
        public object Parameter { get; set; }
    }
}
