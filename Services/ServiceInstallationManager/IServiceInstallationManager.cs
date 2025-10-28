using System;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.ServiceInstallationManager
{
    public interface IServiceInstallationManager
    {
        Task InitializeServiceAsync();
        event Action<string> StepStarted;
    }
}
