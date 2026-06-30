using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Sprout.Core.Services.CPL;
using Sprout.Core.Services.Navigation;
using System.Collections.ObjectModel;
using System.Xml;

namespace Sprout.Core.ViewModels
{
    public partial class ScriptEditorVM : ObservableObject
    {
        private readonly INavigationService _navigationService;

        private BaseCompiler _compiler;

        public TextDocument Document { get; } = new TextDocument();

        public IHighlightingDefinition Highlighting { get; }

        public ScriptEditorVM(INavigationService navigationService)
        {
            _navigationService = navigationService;

            using var stream = GetType().Assembly
                .GetManifestResourceStream("Sprout.Core.CSharpDark.xshd");
            using var reader = XmlReader.Create(stream!);
            Highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }

        // Suggestion lists — populate these to control Ctrl+Space completions
        public ObservableCollection<string> KeywordSuggestions { get; } =
        [
            "class", "interface", "abstract", "override", "virtual",
            "public", "private", "protected", "internal", "static",
            "async", "await", "return", "var", "new", "this", "base",
            "if", "else", "for", "foreach", "while", "switch", "case",
            "break", "continue", "throw", "try", "catch", "finally",
            "using", "namespace", "void", "bool", "int", "string",
            "double", "float", "decimal", "object", "Task"
        ];

        public ObservableCollection<string> TypeSuggestions { get; } =
        [
            "SproutPageVM",
            "CustomPageLogicBase",
            "ObservableCollection",
            "List",
            "Dictionary",
            "IEnumerable",
            "DateTime",
            "Guid",
            "Exception"
        ];

        public ObservableCollection<string> MemberSuggestions { get; } =
        [
            "OnLoadAsync",
            "OnUnloadAsync",
            "OnComponentValueChangedAsync",
            "OnBeforeSaveAsync",
            "Page"
        ];

        [ObservableProperty]
        private bool _hasCompileResult;

        [ObservableProperty]
        private bool _isCompileSuccess;

        [ObservableProperty]
        private string _compileStatusText = string.Empty;

        [ObservableProperty]
        private string _outputText = string.Empty;

        public ObservableCollection<DiagnosticMessage> Diagnostics { get; } = [];

        public void Initialize(BaseCompiler compiler)
        {
            _compiler = compiler;
            Document.Text = _compiler.UserScript ?? string.Empty;

            foreach (var hint in _compiler.GetCompletionHints())
                MemberSuggestions.Add(hint);

            LoadTypeSuggestions();
        }

        // Merges the compiler's currently-imported type names into the global
        // type completion list (idempotent — safe to call again after usings change).
        private void LoadTypeSuggestions()
        {
            if (_compiler is null) return;

            foreach (var typeName in _compiler.GetTypeNames())
                if (!TypeSuggestions.Contains(typeName))
                    TypeSuggestions.Add(typeName);
        }

        [RelayCommand]
        private void ManageUsings()
        {
            if (_compiler is null) return;

            if (_navigationService.ShowManageUsings(_compiler))
                LoadTypeSuggestions();
        }

        // Members available on the expression left of the dot at <caretOffset>,
        // resolved by the compiler's Roslyn semantic model. Handles both static
        // (e.g. "File.") and instance (e.g. "sb.") access.
        public IReadOnlyList<MemberCompletion> GetMemberCompletions(int caretOffset)
            => _compiler?.GetMemberCompletions(Document.Text, caretOffset) ?? [];

        // Method/constructor overloads (and the active parameter) for the call surrounding
        // <caretOffset>, resolved by the compiler. Null when the caret is not inside a call.
        public SignatureHelpResult? GetSignatureHelp(int caretOffset)
            => _compiler?.GetSignatureHelp(Document.Text, caretOffset);

        [RelayCommand]
        private void Save()
        {
            _compiler.UserScript = Document.Text;
            _compiler.SaveUserScript();
        }

        [RelayCommand]
        private void Copy() { }

        [RelayCommand]
        private void CloseOutput()
        {
            HasCompileResult = false;
        }

        [RelayCommand]
        private void Compile()
        {
            _compiler.UserScript = Document.Text;
            var result = _compiler.Compile();

            Diagnostics.Clear();
            foreach (var d in result.Diagnostics)
                Diagnostics.Add(d);

            IsCompileSuccess = result.IsSuccess;
            HasCompileResult = true;

            var errors = result.Diagnostics.Count(d => d.Severity == "Error");
            var warnings = result.Diagnostics.Count(d => d.Severity == "Warning");
            CompileStatusText = result.IsSuccess
                ? $"Build succeeded — {errors} error(s), {warnings} warning(s)"
                : $"Build failed — {errors} error(s), {warnings} warning(s)";

            var sb = new System.Text.StringBuilder();
            foreach (var d in result.Diagnostics)
                sb.AppendLine($"  ({d.Line},{d.Column})  {d.Severity,-8}  {d.Message}");
            OutputText = sb.Length > 0 ? sb.ToString().TrimEnd() : "  No issues found.";
        }
    }
}
