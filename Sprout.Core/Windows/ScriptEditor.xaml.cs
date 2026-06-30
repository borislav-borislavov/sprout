using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Rendering;
using Sprout.Core.Services.CPL;
using Sprout.Core.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Sprout.Core.Windows
{
    public partial class ScriptEditor : Window
    {
        private CompletionWindow? _completionWindow;
        private OverloadInsightWindow? _insightWindow;
        private SignatureHelpProvider? _signatureProvider;

        public ScriptEditor(ScriptEditorVM vm)
        {
            InitializeComponent();
            DataContext = vm;

            // F5 runs the Compile command, mirroring the Compile toolbar button.
            InputBindings.Add(new KeyBinding(vm.CompileCommand, Key.F5, ModifierKeys.None));

            // Ctrl+S runs the Save command, mirroring the Save toolbar button.
            InputBindings.Add(new KeyBinding(vm.SaveCommand, Key.S, ModifierKeys.Control));

            ApplyEditorColors();

            Editor.TextArea.TextEntering += TextArea_TextEntering;
            Editor.TextArea.TextEntered += TextArea_TextEntered;
            Editor.TextArea.KeyDown += TextArea_KeyDown;
            Editor.PreviewMouseWheel += Editor_PreviewMouseWheel;

            vm.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ScriptEditorVM.HasCompileResult)) return;
            if (sender is not ScriptEditorVM vm) return;

            OutputExpander.IsExpanded = vm.HasCompileResult;
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ToolBar toolBar
                && toolBar.Template.FindName("OverflowGrid", toolBar) is FrameworkElement overflow)
                overflow.Visibility = Visibility.Collapsed;
        }

        private void ApplyEditorColors()
        {
            // Selection: VS 2022 blue selection highlight
            Editor.TextArea.SelectionBrush = new SolidColorBrush(Color.FromArgb(80, 65, 140, 215));
            Editor.TextArea.SelectionBorder = new Pen(new SolidColorBrush(Color.FromRgb(65, 140, 215)), 1);
            Editor.TextArea.SelectionForeground = null; // keep syntax colors inside selection

            // Current-line highlight
            Editor.TextArea.TextView.BackgroundRenderers.Add(new CurrentLineHighlightRenderer(Editor));
        }

        public void Initialize(BaseCompiler compiler)
        {
            if (DataContext is ScriptEditorVM vm)
                vm.Initialize(compiler);
        }

        private void TextArea_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                OpenCompletionWindow();
            }
            else if (e.Key == Key.Space
                && e.KeyboardDevice.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                // Ctrl+Shift+Space: show parameter info for the surrounding method call.
                e.Handled = true;
                ShowSignatureHelp();
            }
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (_completionWindow is not null && e.Text.Length > 0)
            {
                if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '_')
                    _completionWindow.CompletionList.RequestInsertion(e);
            }
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            // Auto-open member completions right after a member-access dot, e.g. "File."
            if (e.Text == ".")
            {
                OpenCompletionWindow();
                return;
            }

            // Auto-open parameter info when starting a call, e.g. "Foo(".
            if (e.Text == "(")
            {
                ShowSignatureHelp();
                return;
            }

            // Keep parameter info in sync with the caret while it is open (e.g. after ',').
            if (_insightWindow is not null)
                ShowSignatureHelp();
        }

        private void Editor_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;

            e.Handled = true;
            Editor.FontSize = e.Delta > 0
                ? Math.Min(Editor.FontSize + 1, 40)
                : Math.Max(Editor.FontSize - 1, 8);
        }

        private void OpenCompletionWindow()
        {
            if (DataContext is not ScriptEditorVM vm) return;

            var doc = Editor.Document;

            // Determine the word already typed before the caret
            var offset = Editor.CaretOffset;
            while (offset > 0)
            {
                var c = doc.GetCharAt(offset - 1);
                if (!char.IsLetterOrDigit(c) && c != '_') break;
                offset--;
            }

            var typedText = doc.GetText(offset, Editor.CaretOffset - offset);

            // Detect a member-access context: a '.' immediately precedes the word
            // being typed, e.g. "sb.App|" or "File.".
            var isMemberAccess = offset > 0 && doc.GetCharAt(offset - 1) == '.';

            var items = new List<ScriptCompletionData>();

            if (isMemberAccess)
            {
                // Resolve members through the Roslyn semantic model (instance + static).
                foreach (var member in vm.GetMemberCompletions(Editor.CaretOffset))
                    items.Add(new ScriptCompletionData(member.Name, member.Description));
            }
            else
            {
                // Only show the global list once the user has started typing a word
                if (typedText.Length == 0) return;

                foreach (var kw in vm.KeywordSuggestions)
                    items.Add(new ScriptCompletionData(kw, $"Keyword: {kw}"));

                foreach (var type in vm.TypeSuggestions)
                    items.Add(new ScriptCompletionData(type, $"Type: {type}"));

                foreach (var member in vm.MemberSuggestions)
                    items.Add(new ScriptCompletionData(member, $"Member: {member}"));
            }

            if (items.Count == 0) return;

            _completionWindow = new CompletionWindow(Editor.TextArea)
            {
                Background = new SolidColorBrush(Color.FromRgb(37, 37, 38)),
                Foreground = new SolidColorBrush(Color.FromRgb(212, 212, 212)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
            };

            _completionWindow.CompletionList.Background = new SolidColorBrush(Color.FromRgb(37, 37, 38));
            _completionWindow.CompletionList.Foreground = new SolidColorBrush(Color.FromRgb(212, 212, 212));

            var data = _completionWindow.CompletionList.CompletionData;
            foreach (var item in items)
                data.Add(item);

            _completionWindow.StartOffset = offset;

            _completionWindow.Show();
            _completionWindow.Closed += (_, _) => _completionWindow = null;

            // Filter the list by the word already typed before the caret
            if (typedText.Length > 0)
                _completionWindow.CompletionList.SelectItem(typedText);
        }

        private void ShowSignatureHelp()
        {
            if (DataContext is not ScriptEditorVM vm) return;

            var help = vm.GetSignatureHelp(Editor.CaretOffset);
            if (help is null || help.Signatures.Count == 0)
            {
                CloseSignatureHelp();
                return;
            }

            // Update the open popup in place so the highlighted parameter tracks the caret.
            if (_insightWindow is not null && _signatureProvider is not null)
            {
                _signatureProvider.Update(help);
                return;
            }

            _signatureProvider = new SignatureHelpProvider(help);
            _insightWindow = new OverloadInsightWindow(Editor.TextArea)
            {
                Provider = _signatureProvider,
                Background = new SolidColorBrush(Color.FromRgb(37, 37, 38)),
                Foreground = new SolidColorBrush(Color.FromRgb(212, 212, 212)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
            };

            _insightWindow.Closed += (_, _) =>
            {
                _insightWindow = null;
                _signatureProvider = null;
            };

            _insightWindow.Show();
        }

        private void CloseSignatureHelp() => _insightWindow?.Close();
    }

    // Draws a subtle highlight behind the line the caret is on
    internal sealed class CurrentLineHighlightRenderer : IBackgroundRenderer
    {
        private readonly ICSharpCode.AvalonEdit.TextEditor _editor;
        private static readonly Brush HighlightBrush =
            new SolidColorBrush(Color.FromArgb(18, 255, 255, 255));

        static CurrentLineHighlightRenderer() => HighlightBrush.Freeze();

        public CurrentLineHighlightRenderer(ICSharpCode.AvalonEdit.TextEditor editor)
            => _editor = editor;

        public KnownLayer Layer => KnownLayer.Background;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_editor.Document is null) return;

            textView.EnsureVisualLines();
            var line = _editor.Document.GetLineByOffset(_editor.CaretOffset);

            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, line))
            {
                drawingContext.DrawRectangle(HighlightBrush, null,
                    new Rect(0, rect.Y, textView.ActualWidth, rect.Height));
            }
        }
    }
}

