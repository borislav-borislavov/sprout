using System.Windows;

namespace Sprout.Core.Services.Clipboard
{
    public class ClipboardService : IClipboardService
    {
        public void SetText(string text)
        {
            System.Windows.Clipboard.SetText(text);
        }

        public string GetText()
        {
            return System.Windows.Clipboard.GetText();
        }

        public bool ContainsText()
        {
            return System.Windows.Clipboard.ContainsText();
        }
    }
}
