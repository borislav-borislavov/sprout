using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
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
        protected override string[] Usings =>
        [
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.Threading.Tasks",
            "Sprout.Core.Services.CPL",
            "System.Windows",
            "ClosedXML.Excel"
        ];

        private readonly SproutPageVM _pageVM;
        private readonly IConfigurationService _configurationService;

        public CustomPageLogicCompiler(SproutPageVM vm, IConfigurationService configurationService)
            : base(vm.PageConfig.ID.ToString("N"))
        {
            _pageVM = vm;
            _configurationService = configurationService;
            UserScript = _pageVM.PageConfig.Script;
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

            var sproutConfig = _configurationService.Load();
            var foundPage = sproutConfig.Pages.FirstOrDefault(p => p.ID == _pageVM.PageConfig.ID);
            var pageIndex = sproutConfig.Pages.IndexOf(foundPage);
            sproutConfig.Pages.Remove(foundPage);
            sproutConfig.Pages.Insert(pageIndex, _pageVM.PageConfig);

            _configurationService.Save(sproutConfig);
        }
    }
}
