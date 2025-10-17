using System.Threading;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.WindowsServiceManager
{
    public interface IWindowsServiceManager
    {
        /// <summary>
        /// Checks if the Windows service is currently installed.
        /// </summary>
        /// <param name="cancellationToken">Token for cancellation support.</param>
        Task<bool> IsServiceInstalledAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the Windows service is currently running.
        /// </summary>
        /// <param name="cancellationToken">Token for cancellation support.</param>
        Task<bool> IsServiceRunningAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops and deletes the existing service if present.
        /// </summary>
        /// <param name="cancellationToken">Token for cancellation support.</param>
        Task StopAndDeleteServiceAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Installs the service from the provided executable path.
        /// </summary>
        /// <param name="serviceExePath">Path to the service executable.</param>
        /// <param name="cancellationToken">Token for cancellation support.</param>
        Task InstallServiceAsync(string serviceExePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts the installed service.
        /// </summary>
        /// <param name="cancellationToken">Token for cancellation support.</param>
        Task StartServiceAsync(CancellationToken cancellationToken = default);
    }
}
