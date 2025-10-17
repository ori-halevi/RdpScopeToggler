using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.ServiceExtractor
{
    public class ServiceExtractor : IServiceExtractor
    {
        // Embedded resource names
        private const string RdpScopeServiceExe = "RdpScopeToggler.Assets.Deployment.RdpScopeService.RdpScopeService.exe";
        private const string AppSettingsJson = "RdpScopeToggler.Assets.Deployment.RdpScopeService.appsettings.json";

        public bool Exists(string targetPath)
        {
            return File.Exists(targetPath);
        }

        /// <summary>
        /// Extract both the service executable and appsettings.json
        /// </summary>
        public async Task ExtractAsync(string targetPath, CancellationToken cancellationToken = default)
        {
            // Extract the service executable
            await ExtractResourceAsync(RdpScopeServiceExe, Path.Combine(targetPath, "RdpScopeService.exe"), cancellationToken);

            // Extract the appsettings.json
            await ExtractResourceAsync(AppSettingsJson, Path.Combine(targetPath, "appsettings.json"), cancellationToken);
        }

        /// <summary>
        /// Helper method to extract a single embedded resource
        /// </summary>
        private async Task ExtractResourceAsync(string resourceName, string targetPath, CancellationToken cancellationToken)
        {
            using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

            // Delete the old file if it exists
            if (File.Exists(targetPath))
            {
                try
                {
                    File.Delete(targetPath);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to delete old file '{targetPath}': {ex.Message}");
                }
            }

            // Create the file and copy contents
            await using FileStream file = File.Create(targetPath);
            await stream.CopyToAsync(file, cancellationToken);
        }
    }
}
