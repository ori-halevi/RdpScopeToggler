using System.Threading.Tasks;

namespace RdpScopeToggler.Services.UpdateCheckerService
{
    public interface IUpdateCheckerService
    {
        Task CheckForUpdatesAsync();
    }
}
