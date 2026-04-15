using Sprout.Core.Models.ButtonActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Services.ActionMessageService
{
    public interface IActionMessageService
    {
        public void Show(IButtonActionMessenger buttonActionMessenger);
    }
}
