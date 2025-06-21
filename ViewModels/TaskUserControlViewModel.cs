using GraphicRdpScopeToggler.Services.RdpService;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeToggler.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

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
        private readonly IRegionManager _regionManager;
        private readonly IRdpService rdpService;
        private readonly TaskInfoStore taskInfoStore;

        public TaskUserControlViewModel(IRegionManager regionManager, TaskInfoStore taskInfoStore, IRdpService rdpService)
        {
            this.rdpService = rdpService;
            this.taskInfoStore = taskInfoStore;
            _regionManager = regionManager;

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
            _regionManager.RequestNavigate("ContentRegion", "HomeUserControl");
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
