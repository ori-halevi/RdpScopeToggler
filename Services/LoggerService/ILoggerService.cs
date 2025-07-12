using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.LoggerService
{
    public interface ILoggerService
    {
        public void Info(string message);
        public void Warn(string message);
        public void Error(string message);
        public void Fatal(string message);
        public void Debug(string message);
        public void Error(string message, Exception ex);
        public void Fatal(string message, Exception ex);
        public void OpenLogger();
    }
}
