using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeCommands;
using RdpScopeCommands.Stores;
using RdpScopeToggler.Services.LoggerService;
using RdpScopeToggler.Services.NotificationService;
using RdpScopeToggler.Services.PipeClientService;
using RdpScopeToggler.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        private RdpTask MainTask { get; set; }

        public ICommand DisconnectCommand { get; set; }

        #endregion

        private CancellationTokenSource _cts;
        private readonly IRegionManager regionManager;
        private readonly IRdpController rdpService;
        private readonly TaskInfoStore taskInfoStore;
        private readonly INotificationService notificationService;
        private readonly ILoggerService loggerService;
        private readonly IPipeClientService pipeClientService;
        private bool _isCountingDown = false;

        public TaskUserControlViewModel(IRegionManager regionManager, TaskInfoStore taskInfoStore, IRdpController rdpService,
            INotificationService notificationService, ILoggerService loggerService, IPipeClientService pipeClientService)
        {
            this.pipeClientService = pipeClientService;
            this.loggerService = loggerService;
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
            pipeClientService.ForceExecuteTask(MainTask.Id);
            //regionManager.RequestNavigate("ActionsRegion", "HomeUserControl");
        }

        private async void StartCountingDown()
        {
            if (_isCountingDown) return; // אם כבר רץ – לא להריץ שוב
            _isCountingDown = true;

            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();

                var remaining = new TimeSpan(CountDownDay, CountDownHour, CountDownMinute, CountDownSecond);

                while (remaining.TotalSeconds > 0)
                {
                    if (_cts == null)
                        break;
                    await Task.Delay(1000, _cts.Token);
                    remaining = remaining.Subtract(TimeSpan.FromSeconds(1));

                    CountDownDay = remaining.Days;
                    CountDownHour = remaining.Hours;
                    CountDownMinute = remaining.Minutes;
                    CountDownSecond = remaining.Seconds;

                    // Send notification 5 minutes before closing.
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
                _isCountingDown = false; // שחרור הנעילה
                //regionManager.RequestNavigate("ActionsRegion", "HomeUserControl");
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("task"))
            {
                MainTask = navigationContext.Parameters.GetValue<RdpTask>("task");
                TimeSpan Duration = MainTask.Date - DateTime.Now;
                if (Duration.TotalSeconds <= 0 && MainTask.NextTask != null)
                {
                    MainTask = MainTask.NextTask;
                    Duration = MainTask.Date - DateTime.Now;
                }

                CountDownDay = Duration.Days;
                CountDownHour = Duration.Hours;
                CountDownMinute = Duration.Minutes;
                CountDownSecond = Duration.Seconds;
            }

            var durationString = $"{CountDownDay} days, {CountDownHour} hours, {CountDownMinute} minutes, {CountDownSecond} seconds";
            loggerService.Info($"RDP accessibility window started. The RDP will be available for: {durationString}");

            StartCountingDown();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
