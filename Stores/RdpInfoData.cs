namespace RdpScopeToggler.Stores
{
    public class RdpInfoData
    {
        public bool IsRoleActive { get; set; }
        public bool IsOpenForAll {  get; set; }
        public bool IsOpenOnlyForLocal { get; set; }
        public bool IsOpenForWhiteList { get; set; }

        public RdpInfoData()
        {
            IsRoleActive = false;
            IsOpenForAll = false;
            IsOpenOnlyForLocal = false;
            IsOpenForWhiteList = false;
        }
    }
}
