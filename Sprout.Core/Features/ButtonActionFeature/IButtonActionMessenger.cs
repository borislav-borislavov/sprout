using Sprout.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Features.ButtonActions
{
    public interface IButtonActionMessenger
    {
        List<ActionMessage> Messages { get; }

        void ResetMessages();
    }
}
