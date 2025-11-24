using Microsoft.Toolkit.Uwp.Notifications; // NuGet
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.UpdateCheckerService
{
    public class UpdateCheckerService : IUpdateCheckerService
    {
        private const string RepoApiUrl = "https://api.github.com/repos/ori-halevi/RdpScopeToggler/releases/latest";

        private readonly IHttpClientFactory _httpClientFactory;

        public UpdateCheckerService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task CheckForUpdatesAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request"); // GitHub API דורש User-Agent

                var response = await client.GetAsync(RepoApiUrl);
                if (!response.IsSuccessStatusCode)
                    return;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var latestVersionRaw = doc.RootElement.GetProperty("tag_name").GetString(); // "v.1.1.0"
                var latestVersionString = latestVersionRaw?.TrimStart('v', 'V', '.'); // "1.1.0"
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version; // Version(1.2.0.0)

                if (Version.TryParse(latestVersionString, out var latestVersion) &&
                    latestVersion > currentVersion)
                {
                    ShowUpdateNotification(latestVersion.ToString());
                }
            }
            catch
            {
                Debug.WriteLine("Could not search for better version");
            }


        }

        private void ShowUpdateNotification(string latestVersion)
        {
            new ToastContentBuilder()
                .AddText("Update Available")
                .AddText($"A new version ({latestVersion}) is available on GitHub.")
                .Show(); // מפעיל Toast מיידית
        }
    }
}
