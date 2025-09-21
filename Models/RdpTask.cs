using RdpScopeCommands.Stores;
using System;

namespace RdpScopeToggler.Models
{
    public class RdpTask
    {
        public RdpTask()
        {
            Id = Guid.NewGuid().ToString()[..4];
        }

        public RdpTask(DateTime date, ActionsEnum action, StateEnum state = StateEnum.InQueue, RdpTask nextTask = null)
        {
            Id = Guid.NewGuid().ToString()[..4];
            Date = date;
            Action = action;
            State = state;
            NextTask = nextTask;
        }

        public string Id { get; set; }
        public DateTime Date { get; set; }
        public ActionsEnum Action { get; set; }
        public StateEnum State { get; set; }
        public RdpTask NextTask { get; set; }
        public override string ToString()
        {
            string result = $"[{Id}] {Date:G} | Action: {Action} | State: {State}";

            if (NextTask != null)
                result += $" -> {NextTask}";

            return result;
        }

    }
}
