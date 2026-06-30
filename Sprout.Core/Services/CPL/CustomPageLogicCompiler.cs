using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Windows.Documents;

namespace Sprout.Core.Services.CPL
{
    //from chat: https://claude.ai/chat/825432bf-c079-413b-b608-fa6a47421b49

    public abstract class BaseCompiler
    {
        public string UserScript { get; set; }

        internal string _pageId;

        protected abstract string[] Usings { get; }

        protected BaseCompiler(string pageId)
        {
            _pageId = pageId;
        }

        public CompileResult Compile()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(GetSource());

            var compilation = CSharpCompilation.Create(
                assemblyName: $"PageLogic_{_pageId}_{Guid.NewGuid():N}",
                syntaxTrees: [syntaxTree],
                references: _references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using var ms = new MemoryStream();
            var emit = compilation.Emit(ms);

            var diagnostics = emit.Diagnostics
                .Where(d => d.Severity >= DiagnosticSeverity.Warning)
                .Select(d => new DiagnosticMessage(
                    d.Severity.ToString(),
                    d.GetMessage(),
                    d.Location.GetLineSpan().StartLinePosition.Line + 1,
                    d.Location.GetLineSpan().StartLinePosition.Character + 1)
                ).ToList();

            if (!emit.Success)
                return CompileResult.Failure(diagnostics);

            return CompileResult.Success(ms.ToArray(), diagnostics);
        }

        public abstract string GetSource();
        public abstract void SaveUserScript();

        public virtual IEnumerable<string> GetCompletionHints() => [];

        // Extra, user-managed namespaces imported into the page script. The base
        // implementation has none; page compilers override these to read/apply
        // them from the page configuration.
        public virtual IReadOnlyList<string> GetAdditionalUsings() => [];

        // Applies the edited usings to the in-memory page state only. Persistence is
        // deferred to SaveUserScript so the script editor's Save is the single commit point.
        public virtual void ApplyAdditionalUsings(IEnumerable<string> usings) { }

        // Forces the type-name index to rebuild on next use — call after the set
        // of imported namespaces changes so completion reflects the new usings.
        protected void InvalidateTypeIndex() => _typeIndex = null;

        // Lazily-built map of simple type name -> Type for every public type
        // declared in one of the namespaces imported via Usings.
        private Dictionary<string, Type>? _typeIndex;

        private Dictionary<string, Type> GetTypeIndex()
        {
            if (_typeIndex is not null)
                return _typeIndex;

            var namespaces = new HashSet<string>(Usings, StringComparer.Ordinal);
            var index = new Dictionary<string, Type>(StringComparer.Ordinal);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                Type[] types;
                try
                {
                    types = assembly.GetExportedTypes();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type.Namespace is null || !namespaces.Contains(type.Namespace))
                        continue;

                    var name = type.Name;
                    var tick = name.IndexOf('`');
                    if (tick >= 0)
                        name = name[..tick];

                    index.TryAdd(name, type);
                }
            }

            return _typeIndex = index;
        }

        // All type names available through the imported namespaces.
        public IEnumerable<string> GetTypeNames() => GetTypeIndex().Keys;

        // Resolves the members available on the expression to the left of the dot
        // at <caretOffset> within <userScript>, using a Roslyn semantic model so
        // both static ("File.") and instance ("sb.") access work like real IntelliSense.
        public IReadOnlyList<MemberCompletion> GetMemberCompletions(string userScript, int caretOffset)
        {
            if (string.IsNullOrEmpty(userScript) || caretOffset < 0 || caretOffset > userScript.Length)
                return [];

            // Span of the (possibly partial) member name being typed around the caret.
            var nameStart = caretOffset;
            while (nameStart > 0 && IsIdentifierChar(userScript[nameStart - 1]))
                nameStart--;

            // Member access requires a '.' immediately before the member name.
            if (nameStart == 0 || userScript[nameStart - 1] != '.')
                return [];

            var nameEnd = caretOffset;
            while (nameEnd < userScript.Length && IsIdentifierChar(userScript[nameEnd]))
                nameEnd++;

            // Replace whatever is being typed after the dot with a unique sentinel so
            // the snippet parses as a well-formed member access regardless of context.
            const string sentinel = "__sprout_completion__";
            var modifiedScript = string.Concat(userScript.AsSpan(0, nameStart), sentinel, userScript.AsSpan(nameEnd));

            // Reuse the derived class' source wrapping so locals/usings/page controls
            // are all in scope; swap the script in temporarily for source generation.
            string fullSource;
            var originalScript = UserScript;
            try
            {
                UserScript = modifiedScript;
                fullSource = GetSource();
            }
            finally
            {
                UserScript = originalScript;
            }

            var sentinelPos = fullSource.IndexOf(sentinel, StringComparison.Ordinal);
            if (sentinelPos < 0)
                return [];

            var syntaxTree = CSharpSyntaxTree.ParseText(fullSource);
            var compilation = CSharpCompilation.Create(
                assemblyName: "Completion_" + Guid.NewGuid().ToString("N"),
                syntaxTrees: [syntaxTree],
                references: _references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var model = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();

            var token = root.FindToken(sentinelPos);

            // Depending on the surrounding context Roslyn parses "<receiver>.<sentinel>"
            // either as a member-access expression (value/instance access, e.g. "sb.")
            // or as a qualified name (type/namespace access, e.g. "File."). Handle both
            // so static type access resolves instead of being dropped.
            var sentinelName = token.Parent;
            ExpressionSyntax? receiver = sentinelName?.Parent switch
            {
                MemberAccessExpressionSyntax memberAccess when memberAccess.Name == sentinelName
                    => memberAccess.Expression,
                QualifiedNameSyntax qualifiedName when qualifiedName.Right == sentinelName
                    => qualifiedName.Left,
                _ => null
            };

            if (receiver is null)
                return [];

            // The receiver may have been parsed in a type/namespace context (for example
            // as the left side of a qualified name), where locals, fields and parameters
            // do not bind. Re-bind its text as an expression at the receiver's position so
            // both value receivers ("sb.") and type receivers ("File.") resolve the same
            // way real IntelliSense does.
            var receiverExpression = SyntaxFactory.ParseExpression(receiver.ToString());
            var receiverPosition = receiver.SpanStart;

            // A receiver that binds to a type means static access (File.), otherwise
            // it's an instance and we use the expression's resolved type (sb.).
            var receiverSymbol = model.GetSpeculativeSymbolInfo(
                receiverPosition, receiverExpression, SpeculativeBindingOption.BindAsExpression).Symbol;
            var isStatic = receiverSymbol is INamedTypeSymbol;
            var containerType = isStatic
                ? (INamedTypeSymbol)receiverSymbol!
                : model.GetSpeculativeTypeInfo(
                    receiverPosition, receiverExpression, SpeculativeBindingOption.BindAsExpression).Type;

            if (containerType is null)
                return [];

            var symbols = model.LookupSymbols(
                sentinelPos,
                container: containerType,
                includeReducedExtensionMethods: !isStatic);

            var results = new List<MemberCompletion>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var member in symbols)
            {
                if (!IsCompletableMember(member, isStatic))
                    continue;
                if (!seen.Add(member.Name))
                    continue;

                results.Add(new MemberCompletion(member.Name, DescribeMember(member)));
            }

            results.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return results;
        }

        private static bool IsIdentifierChar(char c) => char.IsLetterOrDigit(c) || c == '_';

        // Keeps only members that make sense in the given (static vs instance) context.
        private static bool IsCompletableMember(ISymbol member, bool isStatic)
        {
            if (member.IsImplicitlyDeclared)
                return false;

            switch (member)
            {
                case IMethodSymbol method:
                    if (method.MethodKind == MethodKind.ReducedExtension)
                        return !isStatic; // extension methods only apply to instances
                    if (method.MethodKind != MethodKind.Ordinary)
                        return false;     // skip ctors, operators, accessors, etc.
                    return method.IsStatic == isStatic;

                case IPropertySymbol property:
                    return property.IsStatic == isStatic;

                case IFieldSymbol field:
                    return (field.IsStatic || field.IsConst) == isStatic;

                case IEventSymbol evt:
                    return evt.IsStatic == isStatic;

                case INamedTypeSymbol:
                    return isStatic; // nested types are reached through the type name

                default:
                    return false;
            }
        }

        private static readonly SymbolDisplayFormat _memberSignatureFormat =
            SymbolDisplayFormat.CSharpErrorMessageFormat;

        private static string DescribeMember(ISymbol member)
            => member.ToDisplayString(_memberSignatureFormat);

        internal string BuildUsings()
        {
            var sb = new StringBuilder();
            foreach (var u in Usings)
                sb.AppendLine($"using {u};");
            return sb.ToString();
        }

        private readonly IReadOnlyList<MetadataReference> _references = BuildReferences();

        private static List<MetadataReference> BuildReferences()
        {
            // Pull in all trusted runtime assemblies
            var refs = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string)!
                .Split(Path.PathSeparator)
                .Select(p => MetadataReference.CreateFromFile(p))
                .Cast<MetadataReference>()
                .ToList();

            // Add your own platform assembly
            refs.Add(MetadataReference.CreateFromFile(typeof(CustomPageLogicBase).Assembly.Location));

            return refs;
        }
    }

    internal class CustomPageLogicCompiler : BaseCompiler
    {
        // Built-in namespaces always available to every page script.
        private static readonly string[] _baseUsings =
        [
            "System",
            "System.Collections.Generic",
            "System.IO",
            "System.Linq",
            "System.Threading.Tasks",
            "Sprout.Core.Services.CPL",
            "System.Windows",
            "ClosedXML.Excel"
        ];

        // Built-in defaults plus the page's extra usings (deduped, defaults win).
        protected override string[] Usings =>
            _baseUsings
                .Concat(_pageVM.PageConfig.Usings ?? [])
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

        private readonly SproutPageVM _pageVM;
        private readonly IConfigurationService _configurationService;

        public CustomPageLogicCompiler(SproutPageVM vm, IConfigurationService configurationService)
            : base(vm.PageConfig.ID.ToString("N"))
        {
            _pageVM = vm;
            _configurationService = configurationService;
            UserScript = _pageVM.PageConfig.Script;
        }

        public override IEnumerable<string> GetCompletionHints()
            => _pageVM.DynamicViewInstance._controls.Keys;

        // Only the user-managed usings (excludes the built-in defaults).
        public override IReadOnlyList<string> GetAdditionalUsings()
            => _pageVM.PageConfig.Usings ?? [];

        public override void ApplyAdditionalUsings(IEnumerable<string> usings)
        {
            _pageVM.PageConfig.Usings = usings
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select(u => u.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            // Applied to the in-memory page config only; saving the script from the
            // editor persists everything together. New namespaces may introduce new
            // types, so refresh the completion index now.
            InvalidateTypeIndex();
        }

        private string BuildPageControlProperties()
        {
            var sb = new StringBuilder();

            foreach (var kvp in _pageVM.DynamicViewInstance._controls)
            {
                var typeName = kvp.Value.GetType().FullName;

                var definition =
                    $"""
                            private {typeName} {kvp.Key} => Page.DynamicViewInstance._controls["{kvp.Key}"] as {typeName};
                    """;

                sb.AppendLine(definition);
            }

            return sb.ToString();
        }

        public override string GetSource()
        {
            var pageControlProperties = BuildPageControlProperties();

            // Wrap user's snippet into a full compilable source file
            // #line 1 resets line numbers so errors point to *user's* lines
            var source =
                $$"""
                {{BuildUsings()}}

                namespace DynamicPageLogic._{{_pageId}}
                {
                    public class CustomPageLogic : CustomPageLogicBase
                    {
                {{pageControlProperties}}
                #line 1 "CustomPageLogic"
                {{UserScript}}
                #line default
                    }
                }
                """;

            return source;
        }

        public override void SaveUserScript()
        {
            _pageVM.PageConfig.Script = UserScript;
            PersistPageConfig();
        }

        // Writes the current page config back into the persisted Sprout config,
        // replacing the stored copy in place.
        private void PersistPageConfig()
        {
            var sproutConfig = _configurationService.Load();
            var foundPage = sproutConfig.Pages.FirstOrDefault(p => p.ID == _pageVM.PageConfig.ID);
            var pageIndex = sproutConfig.Pages.IndexOf(foundPage);
            sproutConfig.Pages.Remove(foundPage);
            sproutConfig.Pages.Insert(pageIndex, _pageVM.PageConfig);

            _configurationService.Save(sproutConfig);
        }
    }
}
