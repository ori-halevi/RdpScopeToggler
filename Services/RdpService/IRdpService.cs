using RdpScopeCommands.Stores;
using RdpScopeToggler.Stores;
using System;

namespace RdpScopeToggler.Services.RdpService
{
    public interface IRdpService
    {
        public event Action RdpDataUpdated;
        public RdpInfoData RdpData { get; }
        public ActionsEnum LastAction { get; }
        
        public void CloseRdpForAllIncludingAlwaysOnList();
        public void OpenRdpForAll();
        public void CloseRdpForAll();
        public void OpenRdpForLocalComputers();
        public void OpenRdpForWhiteList();
        public void OpenRdpForLocalComputersAndForWhiteList();

        public int? GetRdpPort();
        public bool IsRdpPortOpenForLocalhost();
        public void RefreshRdpData();
    }
}
