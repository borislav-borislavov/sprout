using ICSharpCode.AvalonEdit.CodeCompletion;
using Sprout.Core.Services.CPL;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Sprout.Core.Windows
{
    // Bridges the compiler's signature-help data to AvalonEdit's OverloadInsightWindow,
    // rendering each overload with the active parameter highlighted (VS-style parameter info).
    public sealed class SignatureHelpProvider : IOverloadProvider
    {
        private static readonly Brush _foreground = new SolidColorBrush(Color.FromRgb(212, 212, 212));
        private static readonly Brush _activeForeground = Brushes.White;

        private SignatureHelpResult _help;
        private int _selectedIndex;

        public SignatureHelpProvider(SignatureHelpResult help)
        {
            _help = help;
            _selectedIndex = help.ActiveSignature;
        }

        // Refreshes the overloads / active parameter in place as the user keeps typing.
        public void Update(SignatureHelpResult help)
        {
            _help = help;
            if (_selectedIndex >= help.Signatures.Count)
                _selectedIndex = help.ActiveSignature;
            RaiseAllChanged();
        }

        public int Count => _help.Signatures.Count;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                var clamped = value < 0 ? 0 : value >= Count ? Count - 1 : value;
                if (_selectedIndex == clamped)
                    return;
                _selectedIndex = clamped;
                RaiseAllChanged();
            }
        }

        public string CurrentIndexText => $"{_selectedIndex + 1} of {Count}";

        public object CurrentHeader => BuildHeader();

        public object CurrentContent => BuildContent();

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaiseAllChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedIndex)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentIndexText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentHeader)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentContent)));
        }

        private TextBlock BuildHeader()
        {
            var signature = _help.Signatures[_selectedIndex];

            var block = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 640,
                Foreground = _foreground
            };

            block.Inlines.Add(new Run(signature.Prefix));

            for (var i = 0; i < signature.Parameters.Count; i++)
            {
                if (i > 0)
                    block.Inlines.Add(new Run(", "));

                var run = new Run(signature.Parameters[i]);
                if (i == _help.ActiveParameter)
                {
                    run.FontWeight = FontWeights.Bold;
                    run.Foreground = _activeForeground;
                }

                block.Inlines.Add(run);
            }

            block.Inlines.Add(new Run(signature.Suffix));
            return block;
        }

        private object BuildContent()
        {
            var signature = _help.Signatures[_selectedIndex];
            if (_help.ActiveParameter < 0 || _help.ActiveParameter >= signature.Parameters.Count)
                return string.Empty;

            return new TextBlock
            {
                Text = signature.Parameters[_help.ActiveParameter],
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 640,
                Foreground = _foreground
            };
        }
    }
}
