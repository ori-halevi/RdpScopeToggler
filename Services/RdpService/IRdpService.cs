namespace GraphicRdpScopeToggler.Services.RdpService
{
    public interface IRdpService
    {
        public void OpenRdpForAll();
        public void CloseRdpForAll();
        public void OpenRdpForLocalComputers();
        public void OpenRdpForWhiteList();
        public void OpenRdpForLocalComputersAndForWhiteList();
        public int? GetRdpPort();
        public bool IsRdpPortOpen(int port);
    }
}
