namespace RdpScopeToggler.Stores
{
    public class RdpInfoData
    {
        public bool IsRoleActive { get; set; }
        public bool IsOpenForAlwaysOnList { get; set; }
        public bool IsOpenForAll {  get; set; }
        public bool IsOpenForLocalComputers { get; set; }
        public bool IsOpenForLocalComputersAndForWhiteList { get; set; }

        public RdpInfoData()
        {
            IsRoleActive = false;
            IsOpenForAlwaysOnList = false;
            IsOpenForAll = false;
            IsOpenForLocalComputers = false;
            IsOpenForLocalComputersAndForWhiteList = false;
        }
    }
}
