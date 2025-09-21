using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications; // NuGet

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
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("request"); // GitHub API דורש User-Agent

            var response = await client.GetAsync(RepoApiUrl);
            if (!response.IsSuccessStatusCode)
                return;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var latestVersionRaw = doc.RootElement.GetProperty("tag_name").GetString(); // "v.1.1.0"
            var latestVersion = latestVersionRaw?.TrimStart('v', 'V', '.'); // "1.1.0"

            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3); // "1.2.0"

            if (!string.IsNullOrEmpty(latestVersion) && latestVersion != currentVersion)
            {
                ShowUpdateNotification(latestVersion);
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
