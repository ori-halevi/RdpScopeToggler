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

        public async Task RefreshServiceAsync()
        {
            try
            {
                StepStarted?.Invoke(TranslationHelper.Translate("RefreshingService_translator"));

                bool installed = await _serviceManager.IsServiceInstalledAsync();
                bool upToDate = IsServiceUpToDate();

                // Full reinstall path: binary missing, stale, or service not registered.
                // ExtractAsync deletes the on-disk .exe, which fails if the service process
                // still holds it — so we stop & delete first.
                if (!installed || !upToDate)
                {
                    await FullReinstallAsync();

                    bool started = await WaitForServiceStateAsync(expectedRunning: true, timeoutMs: 10000);
                    if (!started)
                        throw new InvalidOperationException("Service failed to start after refresh.");

                    StepStarted?.Invoke(TranslationHelper.Translate("WaitingForService_translator"));
                    return;
                }

                // Clean restart path: binary is current, service is registered.
                if (await _serviceManager.IsServiceRunningAsync())
                {
                    await RunStepAsync(
                        TranslationHelper.Translate("StoppingService_translator"),
                        () => _serviceManager.StopServiceAsync());

                    bool stopped = await WaitForServiceStateAsync(expectedRunning: false, timeoutMs: 10000);

                    // Stop timed out — fall back to hard reset (delete + re-extract + reinstall).
                    if (!stopped)
                    {
                        await FullReinstallAsync();

                        bool restarted = await WaitForServiceStateAsync(expectedRunning: true, timeoutMs: 10000);
                        if (!restarted)
                            throw new InvalidOperationException("Service failed to start after refresh.");

                        StepStarted?.Invoke(TranslationHelper.Translate("WaitingForService_translator"));
                        return;
                    }
                }

                await RunStepAsync(
                    TranslationHelper.Translate("StartingService_translator"),
                    () => _serviceManager.StartServiceAsync());

                bool finalStarted = await WaitForServiceStateAsync(expectedRunning: true, timeoutMs: 10000);
                if (!finalStarted)
                    throw new InvalidOperationException("Service failed to start after refresh.");

                StepStarted?.Invoke(TranslationHelper.Translate("WaitingForService_translator"));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to refresh service: {ex.Message}", ex);
            }
        }

        private async Task FullReinstallAsync()
        {
            await RunStepAsync(
                TranslationHelper.Translate("StoppingAndDeletingService_translator"),
                () => _serviceManager.StopAndDeleteServiceAsync());

            await RunStepAsync(
                TranslationHelper.Translate("ExtractingServiceFiles_translator"),
                () => _serviceExtractor.ExtractAsync(GetServiceFolder()));

            await RunStepAsync(
                TranslationHelper.Translate("InstallingService_translator"),
                () => _serviceManager.InstallServiceAsync(GetServiceExePath()));

            await RunStepAsync(
                TranslationHelper.Translate("StartingService_translator"),
                () => _serviceManager.StartServiceAsync());
        }



        private async Task<bool> WaitForServiceStateAsync(bool expectedRunning, int timeoutMs)
        {
            const int pollInterval = 500;
            int waited = 0;

            while (waited < timeoutMs)
            {
                bool isRunning = await _serviceManager.IsServiceRunningAsync();
                if (isRunning == expectedRunning)
                    return true;

                await Task.Delay(pollInterval);
                waited += pollInterval;
            }

            return false;
        }

        private string GetServiceFolder()
        {
            return Path.Combine("C:", "ProgramData", "RdpScopeToggler", "RdpScopeService");
        }

        private string GetServiceExePath()
        {
            return Path.Combine(GetServiceFolder(), "RdpScopeService.exe");
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
