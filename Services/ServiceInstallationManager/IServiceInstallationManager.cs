using System;
using System.Threading;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.ServiceInstallationManager
{
    public interface IServiceInstallationManager
    {
        Task InitializeServiceAsync();

        Task RefreshServiceAsync();

        event Action<string> StepStarted;
    }
}
