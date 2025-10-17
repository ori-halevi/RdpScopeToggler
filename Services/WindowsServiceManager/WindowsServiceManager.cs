using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.WindowsServiceManager
{
    public class WindowsServiceManager : IWindowsServiceManager
    {
        private const string ServiceName = "RdpScopeService";

        private async Task RunScCommandAsync(string args, CancellationToken cancellationToken = default)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = args,
                UseShellExecute = false,
                Verb = "runas",
                CreateNoWindow = true
            });

            if (process == null)
                throw new InvalidOperationException("Failed to start sc.exe process.");

            await process.WaitForExitAsync(cancellationToken);
        }

        public async Task<bool> IsServiceInstalledAsync(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
                ServiceController.GetServices().Any(s => s.ServiceName.Equals(ServiceName, StringComparison.OrdinalIgnoreCase)),
                cancellationToken);
        }

        public async Task<bool> IsServiceRunningAsync(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var svc = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName.Equals(ServiceName, StringComparison.OrdinalIgnoreCase));
                return svc != null && svc.Status == ServiceControllerStatus.Running;
            }, cancellationToken);
        }

        public async Task StopAndDeleteServiceAsync(CancellationToken cancellationToken = default)
        {
            if (!await IsServiceInstalledAsync(cancellationToken))
                return;

            try
            {
                // עצירה
                await RunScCommandAsync($"stop {ServiceName}", cancellationToken);
                await Task.Delay(1500, cancellationToken);

                // מחיקה
                await RunScCommandAsync($"delete {ServiceName}", cancellationToken);

                // המתן לוודאות שהתהליך נעלם
                await Task.Delay(2500, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ServiceManager] Failed to stop/delete service: {ex.Message}");
            }
        }

        public async Task InstallServiceAsync(string serviceExePath, CancellationToken cancellationToken = default)
        {
            await RunScCommandAsync($"create {ServiceName} binPath= \"{serviceExePath}\" start= auto", cancellationToken);
            await Task.Delay(1000, cancellationToken);
        }

        public async Task StartServiceAsync(CancellationToken cancellationToken = default)
        {
            await RunScCommandAsync($"start {ServiceName}", cancellationToken);
        }
    }
}

