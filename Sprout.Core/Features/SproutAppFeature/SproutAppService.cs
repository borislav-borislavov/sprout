using Sprout.Core.Common;
using Sprout.Core.Features.AppStateFeature;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Sprout.Core.Features.SproutAppFeature;

public class SproutAppService : ISproutAppService
{
    private readonly IAppState _appState;

    public SproutAppService(IAppState appState)
    {
        _appState = appState;
    }

    public void CloseApp(bool force)
    {
        _appState.Set(Const.AppState.ForceCloseApp, force);

        //this prompts the user to close the app and it should be done automatically
        Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
    }

    public void StartApp(string? arguments = null)
    {
        string exePath = GetExeFilePath()
            ?? throw new InvalidOperationException("Unable to determine the executable path.");

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = Path.GetDirectoryName(exePath)!,
            UseShellExecute = true
        };

        if (!string.IsNullOrWhiteSpace(arguments))
        {
            startInfo.Arguments = arguments;
        }

        Process.Start(startInfo);
    }

    public string? GetExeFilePath() => Process.GetCurrentProcess().MainModule?.FileName;

}