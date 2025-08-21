using RdpScopeCommands.Stores;
using System;

namespace RdpScopeToggler.Stores
{
    public class TaskInfoStore
    {
        public TimeSpan Duration { get; set; }
        public ActionsEnum Action { get; set; }
        public DateTime Date { get; set; }
    }
}
