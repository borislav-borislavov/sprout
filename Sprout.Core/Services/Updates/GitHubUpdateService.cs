using Sprout.Core.Common;
using Sprout.Core.Services.Dialog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace Sprout.Core.Services.Updates
{
    public class GitHubUpdateService : IUpdateService
    {
        private const string OldExecutableSuffix = ".old";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDialogService _dialogService;

        public GitHubUpdateService(IHttpClientFactory httpClientFactory, IDialogService dialogService)
        {
            _httpClientFactory = httpClientFactory;
            _dialogService = dialogService;
        }

        public async Task CheckForUpdatesAsync()
        {
            try
            {
                CleanUpOldExecutable();

                var exePath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exePath))
                {
                    _dialogService.ShowError("Unable to determine the application executable path.");
                    return;
                }

                var currentVersionStr = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly()!.Location).FileVersion;

                if (currentVersionStr == null)
                {
                    _dialogService.ShowError("Unable to determine the current Sprout version.");
                    return;
                }

                var currentVersion = Version.Parse(currentVersionStr);

                var testClient = _httpClientFactory.CreateClient();
                var latestUrl = await testClient.GetStringAsync("https://github.com/borislav-borislavov/sprout/releases/download/registry/latest.txt");

                if (string.IsNullOrEmpty(latestUrl))
                {
                    _dialogService.ShowError($"The release version 'https://github.com/borislav-borislavov/sprout/releases/download/registry/latest.txt' is not valid.");
                    return;
                }

                var versionWip = latestUrl.StartFrom("download/", true);

                if (string.IsNullOrEmpty(versionWip))
                {
                    _dialogService.ShowError($"The release version '{versionWip}' is not valid.");
                    return;
                }

                versionWip = versionWip.StopAt("/");

                if (string.IsNullOrEmpty(versionWip))
                {
                    _dialogService.ShowError($"The release version '{versionWip}' is not valid.");
                    return;
                }

                if (!Version.TryParse(versionWip, out var latestVersion))
                {
                    _dialogService.ShowError($"The release version '{versionWip}' is not valid.");
                    return;
                }


                if (latestVersion <= currentVersion)
                {
                    _dialogService.ShowMessage(
                        $"Sprout is up to date. You are running version {currentVersionStr}.",
                        "No updates available");
                    return;
                }

                var result = _dialogService.ShowMessage(
                    $"A new version of Sprout is available ({latestVersion}). You are running {currentVersionStr}.{Environment.NewLine}Do you want to update now?",
                    "Update available",
                    DialogButton.YesNo);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                var downloadPath = exePath + ".update";
                await DownloadAsync(testClient, latestUrl, downloadPath);

                ApplyUpdate(exePath, downloadPath);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Unable to check for updates.{Environment.NewLine}{ex.Message}");
            }
        }

        private static async Task DownloadAsync(HttpClient client, string url, string destinationPath)
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var fileStream = File.Create(destinationPath);
            await response.Content.CopyToAsync(fileStream);
        }

        private static void ApplyUpdate(string exePath, string downloadPath)
        {
            var oldPath = exePath + OldExecutableSuffix;

            if (File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }

            // The running executable cannot be overwritten, but it can be renamed.
            File.Move(exePath, oldPath);
            File.Move(downloadPath, exePath);

            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = Path.GetDirectoryName(exePath)!,
                UseShellExecute = true
            });

            Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
        }

        private static void CleanUpOldExecutable()
        {
            try
            {
                var exePath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exePath))
                {
                    return;
                }

                var oldPath = exePath + OldExecutableSuffix;
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }
            }
            catch
            {
                // The old executable may still be locked by the previous instance; it will be cleaned up next time.
            }
        }

    }
}
