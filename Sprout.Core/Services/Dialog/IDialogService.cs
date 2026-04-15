using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sprout.Core.Services.Dialog
{
    public interface IDialogService
    {
        DialogResult ShowMessage(string message);
        DialogResult ShowMessage(string message, DialogButton dialogButton);
        DialogResult ShowMessage(string message, string caption);
        DialogResult ShowMessage(string message, string caption, DialogButton dialogButton);
        DialogResult ShowMessage(string message, string caption, DialogButton dialogButton, DialogImage dialogImage);
        DialogResult ShowError(string message);
        DialogResult ShowWarning(string message);
    }
}
