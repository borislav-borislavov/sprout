namespace Sprout.Core.Services.Clipboard
{
    public interface IClipboardService
    {
        void SetText(string text);
        void SetHtml(string html);
        string GetText();
        bool ContainsText();
    }
}
