using RdpScopeToggler.Stores;
using System;
using System.Collections.Generic;

namespace RdpScopeToggler.Models
{
    public class ServiceMessage
    {
        public RdpTask CurrentTask { get; set; } = new RdpTask();
        public RdpInfoData CurrentRdpState { get; set; } = new RdpInfoData();
        public DateTime TimeStemp { get; set; } = DateTime.UtcNow;
        public string? Error { get; set; } = null;
        public List<Client>? WhiteList { get; set; } = null;
        public List<Client>? AlwaysTrustedList { get; set; } = null;
    }
}
