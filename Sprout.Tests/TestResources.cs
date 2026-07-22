using System;
using System.Windows;

namespace Sprout.Tests
{
    /// <summary>
    /// Test setup helper that makes the application-level WPF resources
    /// (defined in Sprout.Core\GlobalResources.xaml) available during unit tests.
    ///
    /// At runtime these resources are merged by Sprout.Shell\App.xaml. Unit tests
    /// have no Application instance, so StaticResource lookups such as
    /// {StaticResource RoundBtnHover} fail. Call <see cref="EnsureLoaded"/> from the
    /// Arrange section of any WPF test that creates controls depending on them.
    /// </summary>
    public static class TestResources
    {
        private static readonly object _sync = new();
        private static bool _loaded;

        private const string GlobalResourcesUri =
            "pack://application:,,,/Sprout.Core;component/GlobalResources.xaml";

        public static void EnsureLoaded()
        {
            lock (_sync)
            {
                if (_loaded)
                    return;

                // A WPF Application owns the application-scoped ResourceDictionary
                // that StaticResource falls back to. Create one if a host app
                // (e.g. the real shell) has not already provided it.
                if (Application.Current is null)
                    _ = new Application();

                var globalResources = new ResourceDictionary
                {
                    Source = new Uri(GlobalResourcesUri, UriKind.Absolute)
                };

                Application.Current.Resources.MergedDictionaries.Add(globalResources);

                _loaded = true;
            }
        }
    }
}
