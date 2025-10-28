using System.Threading;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.ServiceExtractor
{
    public interface IServiceExtractor
    {
        /// <summary>
        /// Extracts the embedded Windows service executable to the target path.
        /// If a previous version exists, it is overwritten.
        /// </summary>
        /// <param name="targetPath">The full path to extract the service executable to.</param>
        /// <param name="cancellationToken">Token for cancellation support.</param>
        Task ExtractAsync(string targetPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether the embedded service exists at the target location.
        /// </summary>
        /// <param name="targetPath">The expected path of the service executable.</param>
        /// <returns>True if the file exists, false otherwise.</returns>
        bool Exists(string targetPath);
    }
}
