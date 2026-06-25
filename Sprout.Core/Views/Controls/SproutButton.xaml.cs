using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
#nullable disable

namespace Sprout.Core.Views.Controls
{
    /// <summary>
    /// Interaction logic for SproutButton.xaml
    /// </summary>
    public partial class SproutButton : UserControl, ISproutControl<SproutButtonConfig>
    {
        public SproutButtonConfig Config { get; set; }
        public SproutControlType ControlType => SproutControlType.Button;

        // ── ButtonContent ──────────────────────────────────────────────────────
        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register(nameof(ButtonContent), typeof(string), typeof(SproutButton),
                new PropertyMetadata(string.Empty));

        public string ButtonContent
        {
            get => (string)GetValue(ButtonContentProperty);
            set => SetValue(ButtonContentProperty, value);
        }

        // ── Icon ───────────────────────────────────────────────────────────────
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(string), typeof(SproutButton),
                new PropertyMetadata(string.Empty));

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        // ── IconFont ───────────────────────────────────────────────────────────
        public static readonly DependencyProperty IconFontProperty =
            DependencyProperty.Register(nameof(IconFont), typeof(string), typeof(SproutButton),
                new PropertyMetadata("Segoe MDL2 Assets"));

        public string IconFont
        {
            get => (string)GetValue(IconFontProperty);
            set => SetValue(IconFontProperty, value);
        }

        // ── IconFontSize ───────────────────────────────────────────────────────
        public static readonly DependencyProperty IconFontSizeProperty =
            DependencyProperty.Register(nameof(IconFontSize), typeof(double), typeof(SproutButton),
                new PropertyMetadata(14.0));

        public double IconFontSize
        {
            get => (double)GetValue(IconFontSizeProperty);
            set => SetValue(IconFontSizeProperty, value);
        }

        // ── TextFontSize ───────────────────────────────────────────────────────
        public static readonly DependencyProperty TextFontSizeProperty =
            DependencyProperty.Register(nameof(TextFontSize), typeof(double), typeof(SproutButton),
                new PropertyMetadata(12.0));

        public double TextFontSize
        {
            get => (double)GetValue(TextFontSizeProperty);
            set => SetValue(TextFontSizeProperty, value);
        }

        // ── BrushThickness ─────────────────────────────────────────────────────
        public static readonly DependencyProperty BrushThicknessProperty =
            DependencyProperty.Register(nameof(BrushThickness), typeof(int), typeof(SproutButton),
                new PropertyMetadata(1, OnBrushThicknessChanged));

        private static void OnBrushThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SproutButton sb)
                sb.button.BorderThickness = new Thickness((int)e.NewValue);
        }

        public int BrushThickness
        {
            get => (int)GetValue(BrushThicknessProperty);
            set => SetValue(BrushThicknessProperty, value);
        }

        // ── ForegroundColor ────────────────────────────────────────────────────
        public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.Register(nameof(ForegroundColor), typeof(string), typeof(SproutButton),
                new PropertyMetadata(null, OnForegroundColorChanged));

        private static void OnForegroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SproutButton sb) return;

            var colorString = e.NewValue as string;

            if (!string.IsNullOrWhiteSpace(colorString))
            {
                try
                {
                    var brush = (Brush)new BrushConverter().ConvertFromString(colorString);
                    sb.iconBlock.Foreground = brush;
                    sb.textBlock.Foreground = brush;
                    return;
                }
                catch { /* invalid value — fall through and clear */ }
            }

            // Null/empty/invalid: clear so PrimaryFontBrush is inherited from the global style
            sb.iconBlock.ClearValue(ForegroundProperty);
            sb.textBlock.ClearValue(ForegroundProperty);
        }

        public string ForegroundColor
        {
            get => (string)GetValue(ForegroundColorProperty);
            set => SetValue(ForegroundColorProperty, value);
        }

        public SproutButton()
        {
            InitializeComponent();
            // Stamp as a local value so it always wins over the style's BorderThickness="0".
            // The callback handles later changes, but WPF skips the callback when the value
            // matches the DP default — so we guarantee it here too.
            button.BorderThickness = new Thickness(BrushThickness);
        }
    }
}
