using Sprout.Core;
using Sprout.Core.Features.LogFeature;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sprout.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            #region Focus text of textbox when focused (Works on all textboxes)
            EventManager.RegisterClassHandler(typeof(TextBox),
                UIElement.GotFocusEvent,
                new RoutedEventHandler((s, _) => (s as TextBox)?.SelectAll()));

            EventManager.RegisterClassHandler(typeof(TextBox),
                UIElement.PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler((s, ev) =>
                {
                    if (s is TextBox textBox && !textBox.IsKeyboardFocusWithin)
                    {
                        textBox.Focus();
                        ev.Handled = true;
                    }
                }));
            #endregion

            base.OnStartup(e);

            SproutApp.Start();
        }
    }
}
