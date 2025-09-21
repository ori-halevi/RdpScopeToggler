using RdpScopeCommands.Stores;
using System;

namespace RdpScopeToggler.Models
{
    public class ServiceMessage
    {
        public RdpTask CurrentTask { get; set; } = new RdpTask();
        public RdpInfoData CurrentRdpState { get; set; } = new RdpInfoData();
        public DateTime TimeStemp { get; set; } = DateTime.UtcNow;
    }
}
