using RdpScopeToggler.Helpers;
using RdpScopeToggler.Services.ServiceExtractor;
using RdpScopeToggler.Services.WindowsServiceManager;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.ServiceInstallationManager
{
    public class ServiceInstallationManager : IServiceInstallationManager
    {
        private readonly IWindowsServiceManager _serviceManager;
        private readonly IServiceExtractor _serviceExtractor;

        public event Action<string> StepStarted;

        public ServiceInstallationManager(
            IWindowsServiceManager serviceManager,
            IServiceExtractor serviceExtractor)
        {
            _serviceManager = serviceManager;
            _serviceExtractor = serviceExtractor;
        }

        public async Task InitializeServiceAsync()
        {
            try
            {
                StepStarted?.Invoke(TranslationHelper.Translate("WaitingForService_translator"));

                string servicePath = Path.Combine("C:", "ProgramData", "RdpScopeToggler", "RdpScopeService");

                // Check if the latest updated service is already installed
                if (!IsServiceUpToDate())
                {
                    await RunStepAsync(TranslationHelper.Translate("StoppingAndDeletingService_translator"),
                        () => _serviceManager.StopAndDeleteServiceAsync());

                    await RunStepAsync(TranslationHelper.Translate("ExtractingServiceFiles_translator"),
                        () => _serviceExtractor.ExtractAsync(servicePath));

                    await RunStepAsync(TranslationHelper.Translate("InstallingService_translator"),
                        () => _serviceManager.InstallServiceAsync(Path.Combine(servicePath, "RdpScopeService.exe")));

                    await RunStepAsync(TranslationHelper.Translate("StartingService_translator"),
                        () => _serviceManager.StartServiceAsync());

                    StepStarted?.Invoke(TranslationHelper.Translate("WaitingForService_translator"));
                }
                else if (!await _serviceManager.IsServiceInstalledAsync())
                {
                    await RunStepAsync(TranslationHelper.Translate("InstallingService_translator"),
                        () => _serviceManager.InstallServiceAsync(Path.Combine(servicePath, "RdpScopeService.exe")));

                    await RunStepAsync(TranslationHelper.Translate("StartingService_translator"),
                        () => _serviceManager.StartServiceAsync());

                    StepStarted?.Invoke(TranslationHelper.Translate("WaitingForService_translator"));
                }
                else if (!await _serviceManager.IsServiceRunningAsync())
                {
                    await RunStepAsync(TranslationHelper.Translate("StartingService_translator"),
                        () => _serviceManager.StartServiceAsync());
                }
                
                StepStarted?.Invoke(TranslationHelper.Translate("WaitingForService_translator"));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize service: {ex.Message}", ex);
            }
        }

        private async Task RunStepAsync(string description, Func<Task> action)
        {
            StepStarted?.Invoke(description);
            await action();
        }



        private bool IsServiceUpToDate()
        {
            string servicePath = Path.Combine("C:", "ProgramData", "RdpScopeToggler", "RdpScopeService");
            string installedService = Path.Combine(servicePath, "RdpScopeService.exe");

            if (!File.Exists(installedService))
                return false;

            // Load embedded resource to temp file
            string tempPath = Path.GetTempFileName();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RdpScopeToggler.Assets.Deployment.RdpScopeService.RdpScopeService.exe"))
            using (var fileStream = File.Create(tempPath))
            {
                stream.CopyTo(fileStream);
            }

            string installedHash = ComputeHash(installedService);
            string resourceHash = ComputeHash(tempPath);

            File.Delete(tempPath);

            return installedHash == resourceHash;
        }

        private string ComputeHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hashBytes = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
