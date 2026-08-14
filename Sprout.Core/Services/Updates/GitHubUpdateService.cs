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
        private const string LatestReleaseUrl = "https://api.github.com/repos/borislav-borislavov/sprout/releases/latest";
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
                //var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version;
                
                if (currentVersionStr == null)
                {
                    _dialogService.ShowError("Unable to determine the current Sprout version.");
                    return;
                }

                var currentVersion = Version.Parse(currentVersionStr);

                using var client = CreateClient();

                var release = await client.GetFromJsonAsync<GitHubRelease>(LatestReleaseUrl);
                if (release?.TagName == null)
                {
                    _dialogService.ShowError("The latest GitHub release does not have a version tag.");
                    return;
                }

                if (!Version.TryParse(release.TagName.TrimStart('v', 'V'), out var latestVersion))
                {
                    _dialogService.ShowError($"The release version '{release.TagName}' is not valid.");
                    return;
                }

                if (latestVersion <= currentVersion)
                {
                    _dialogService.ShowMessage(
                        $"Sprout is up to date. You are running version {currentVersionStr}.",
                        "No updates available");
                    return;
                }

                var asset = release.Assets?.FirstOrDefault(a =>
                    a.Name?.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true);

                if (asset?.BrowserDownloadUrl == null)
                {
                    _dialogService.ShowError("The latest GitHub release does not contain a Sprout executable.");
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
                await DownloadAsync(client, asset.BrowserDownloadUrl, downloadPath);

                ApplyUpdate(exePath, downloadPath);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Unable to check for updates.{Environment.NewLine}{ex.Message}");
            }
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Sprout", "1.0"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            return client;
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

        private class GitHubRelease
        {
            [JsonPropertyName("tag_name")]
            public string? TagName { get; set; }

            [JsonPropertyName("assets")]
            public GitHubAsset[]? Assets { get; set; }
        }

        private class GitHubAsset
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("browser_download_url")]
            public string? BrowserDownloadUrl { get; set; }
        }
    }
}
