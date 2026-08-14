using Microsoft.Extensions.DependencyInjection;
using Sprout.Core;
using Sprout.Core.Windows;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Sprout.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string GlobalExceptionsFileName = "GlobalExceptions.txt";
        private static readonly object _exceptionLogSync = new();

        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
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

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            WriteException("Dispatcher", e.Exception);
        }

        private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            WriteException("AppDomain", e.ExceptionObject);
        }

        private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            WriteException("TaskScheduler", e.Exception);
        }

        private static void WriteException(string source, object exception)
        {
            try
            {
                var entry = new StringBuilder()
                    .Append('[').Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("] ")
                    .Append('[').Append(source).AppendLine("]")
                    .AppendLine(exception.ToString())
                    .AppendLine(new string('-', 80))
                    .ToString();

                lock (_exceptionLogSync)
                {
                    var logPath = Path.Combine(Environment.CurrentDirectory, GlobalExceptionsFileName);
                    File.AppendAllText(logPath, entry, Encoding.UTF8);
                }
            }
            catch
            {
                // Exception logging must not replace the original exception.
            }
        }
    }
}
