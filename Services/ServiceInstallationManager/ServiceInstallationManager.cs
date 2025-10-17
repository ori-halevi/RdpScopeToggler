using RdpScopeToggler.Helpers;
using RdpScopeToggler.Services.ServiceExtractor;
using RdpScopeToggler.Services.WindowsServiceManager;
using System;
using System.IO;
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
                string servicePath = Path.Combine("C:", "ProgramData", "RdpScopeToggler", "RdpScopeService");

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
    }
}
