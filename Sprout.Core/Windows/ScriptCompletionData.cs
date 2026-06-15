using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows.Media;

namespace Sprout.Core.Windows
{
    public class ScriptCompletionData : ICompletionData
    {
        public ScriptCompletionData(string text, string? description = null)
        {
            Text = text;
            Description = description ?? text;
        }

        public ImageSource? Image => null;

        public string Text { get; }

        public object Content => Text;

        public object Description { get; }

        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
}
