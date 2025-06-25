using RdpScopeToggler.Services.RdpService;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeToggler.Stores;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using RdpScopeToggler.Services.NotificationService;

namespace RdpScopeToggler.ViewModels
{
    public class TaskUserControlViewModel : BindableBase, INavigationAware
    {
        #region Properties
        private int countDownDay;
        public int CountDownDay
        {
            get { return countDownDay; }
            set { SetProperty(ref countDownDay, value); }
        }
        private int countDownHour;
        public int CountDownHour
        {
            get { return countDownHour; }
            set { SetProperty(ref countDownHour, value); }
        }

        private int countDownMinute;
        public int CountDownMinute
        {
            get { return countDownMinute; }
            set { SetProperty(ref countDownMinute, value); }
        }
        private int countDownSecond;
        public int CountDownSecond
        {
            get { return countDownSecond; }
            set { SetProperty(ref countDownSecond, value); }
        }

        public ICommand DisconnectCommand { get; set; }

        #endregion

        private CancellationTokenSource _cts;
        private readonly IRegionManager regionManager;
        private readonly IRdpService rdpService;
        private readonly TaskInfoStore taskInfoStore;
        private readonly INotificationService notificationService;

        public TaskUserControlViewModel(IRegionManager regionManager, TaskInfoStore taskInfoStore, IRdpService rdpService, INotificationService notificationService)
        {
            this.rdpService = rdpService;
            this.taskInfoStore = taskInfoStore;
            this.notificationService = notificationService;
            this.regionManager = regionManager;

            DisconnectCommand = new DelegateCommand(Disconnect);
        }

        private void Disconnect()
        {
            if (_cts != null)
            {
                _cts.Cancel();     // מבטל את המשימה הרצה
                _cts.Dispose();    // משחרר משאבים
                _cts = null;       // מוחק את ההתייחסות
            }
        }

        private async void StartCountingDown()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            var remaining = new TimeSpan(CountDownDay, CountDownHour, CountDownMinute, CountDownSecond);

            try
            {
                // פעולה בתחילת הטיימר
                OpenConnection();

                while (remaining.TotalSeconds > 0)
                {
                    await Task.Delay(1000, _cts.Token);
                    remaining = remaining.Subtract(TimeSpan.FromSeconds(1));

                    CountDownDay = remaining.Days;
                    CountDownHour = remaining.Hours;
                    CountDownMinute = remaining.Minutes;
                    CountDownSecond = remaining.Seconds;

                    if (remaining.TotalSeconds == 60 * 5)
                        notificationService.SendPreDisconnectAlert();
                }
            }
            catch (TaskCanceledException)
            {
                // הספירה בוטלה
            }
            finally
            {
                if (_cts != null)
                {
                    _cts.Dispose();    // משחרר משאבים
                    _cts = null;       // מוחק את ההתייחסות
                }

                // פעולה בסיום טיימר / ביטול
                CloseConnection();
            }
        }


        private void CloseConnection()
        {
            rdpService.OpenRdpForLocalComputers();
            regionManager.RequestNavigate("ContentRegion", "HomeUserControl");
        }

        private void OpenConnection()
        {
            if (taskInfoStore.Action == ActionsEnum.WhiteList)
                rdpService.OpenRdpForLocalComputersAndForWhiteList();
            if (taskInfoStore.Action == ActionsEnum.RemoteSystems)
                rdpService.OpenRdpForAll();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            CountDownDay = taskInfoStore.Duration.Days;
            CountDownHour = taskInfoStore.Duration.Hours;
            CountDownMinute = taskInfoStore.Duration.Minutes;
            CountDownSecond = taskInfoStore.Duration.Seconds;

            StartCountingDown();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
