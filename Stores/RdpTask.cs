using RdpScopeCommands.Stores;
using System;

namespace RdpScopeToggler.Stores
{
    public class RdpTask
    {
        public required string Id { get; set; }
        public DateTime Date { get; set; }
        public ActionsEnum Action { get; set; }
        public StateEnum State { get; set; }
        public RdpTask? NextTask { get; set; }
    }
}
