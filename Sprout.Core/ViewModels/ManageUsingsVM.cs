using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Services.CPL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sprout.Core.ViewModels
{
    public partial class ManageUsingsVM : ObservableObject
    {
        private BaseCompiler _compiler;

        // One namespace per line; forgiving of pasted "using X.Y;" syntax.
        [ObservableProperty]
        private string _usingsText = string.Empty;

        public void Initialize(BaseCompiler compiler)
        {
            _compiler = compiler;
            UsingsText = string.Join(Environment.NewLine, compiler.GetAdditionalUsings());
        }

        // Applies the edited usings to the page in memory. They are persisted only
        // when the user saves from the script editor.
        [RelayCommand]
        private void Apply()
        {
            _compiler?.ApplyAdditionalUsings(ParseUsings(UsingsText));
        }

        // Turns the multiline editor text into clean namespace tokens, tolerating
        // optional "using" prefixes and trailing semicolons.
        private static IEnumerable<string> ParseUsings(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return [];

            var seen = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<string>();

            foreach (var rawLine in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            {
                var line = rawLine.Trim();

                if (line.StartsWith("using ", StringComparison.Ordinal))
                    line = line["using ".Length..].Trim();

                line = line.TrimEnd(';').Trim();

                if (line.Length > 0 && seen.Add(line))
                    result.Add(line);
            }

            return result;
        }
    }
}
