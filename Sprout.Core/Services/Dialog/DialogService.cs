using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sprout.Core.Services.Dialog
{
    public class DialogService : IDialogService
    {
        public DialogResult ShowMessage(string message, string caption, DialogButton dialogButton, DialogImage dialogImage)
        {
            return (DialogResult)MessageBox.Show(message, caption, (MessageBoxButton)dialogButton, (MessageBoxImage)dialogImage); ;
        }

        public DialogResult ShowMessage(string message, string caption, DialogButton dialogButton)
        {
            return ShowMessage(message, caption, dialogButton, DialogImage.None);
        }

        public DialogResult ShowMessage(string message, string caption)
        {
            return ShowMessage(message, caption, DialogButton.OK);
        }

        public DialogResult ShowMessage(string message)
        {
            return ShowMessage(message, string.Empty);
        }

        public DialogResult ShowMessage(string message, DialogButton dialogButton)
        {
            return ShowMessage(message, string.Empty, dialogButton);
        }
    }
}
