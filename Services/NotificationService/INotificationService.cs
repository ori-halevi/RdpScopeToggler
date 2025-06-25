using System;

namespace RdpScopeToggler.Services.NotificationService
{
    public interface INotificationService
    {
        public event Action NotificationToolInstalled;
        public void InitializeInstallation();
        public void SendPreDisconnectAlert();
    }
}
