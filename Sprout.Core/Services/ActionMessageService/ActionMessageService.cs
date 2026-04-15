using Sprout.Core.Models.ButtonActions;
using Sprout.Core.Services.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Services.ActionMessageService
{
    public class ActionMessageService(IDialogService _dialogService) : IActionMessageService
    {
        public void Show(IButtonActionMessenger buttonActionMessenger)
        {
            foreach (var actionMessage in buttonActionMessenger.Messages)
            {
                if (actionMessage.Type == "Info")
                {
                    _dialogService.ShowMessage(actionMessage.Message, "Info");
                }
                else if (actionMessage.Type == "Warning")
                {
                    _dialogService.ShowWarning(actionMessage.Message);
                }
                else if (actionMessage.Type == "Error")
                {
                    _dialogService.ShowError(actionMessage.Message);
                }
                else
                {
                    _dialogService.ShowMessage(actionMessage.Message);
                }
            }
        }
    }
}
