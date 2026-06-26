namespace Sprout.Core.Services.Clipboard
{
    public interface IClipboardService
    {
        void SetText(string text);
        string GetText();
        bool ContainsText();
    }
}
