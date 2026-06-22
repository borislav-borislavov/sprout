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

        public SproutButton()
        {
            InitializeComponent();
        }
    }
}
