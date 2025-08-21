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
using RdpScopeToggler.Services.LoggerService;
using System.Windows;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using RdpScopeCommands;
using static System.Windows.Forms.AxHost;
using RdpScopeCommands.Stores;

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

        public TaskUserControlViewModel(IRegionManager regionManager, TaskInfoStore taskInfoStore, IRdpController rdpService, INotificationService notificationService, ILoggerService loggerService)
        {
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
            CancelTask();
            regionManager.RequestNavigate("ActionsRegion", "HomeUserControl");
        }

        private void CancelTask()
        {
            WriteToFile(MainTask);
        }
        private void WriteToFile(RdpTask mainTask)
        {
            try
            {
                string filePath = @"C:\ProgramData\RdpScopeToggler\Tasks.json";

                // הגדרת JsonSerializer עם פורמט מותאם לתאריכים
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters =
                    {
                        new JsonStringEnumConverter(),       // <--- מאפשר המרה בין Enum למחרוזת
                    }
                };

                // קרא את הקובץ אם קיים
                List<RdpTask> tasks;
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    tasks = JsonSerializer.Deserialize<List<RdpTask>>(json, options) ?? new List<RdpTask>();
                }
                else
                {
                    tasks = new List<RdpTask>();
                }

                // ערוך את הstate של המשימות
                foreach (var task in tasks)
                {
                    if (task.Id.Equals(mainTask.Id))
                    {
                        task.State = StateEnum.Canceled;
                    }
                }


                var newTask = new RdpTask
                {
                    Id = Guid.NewGuid().ToString(),
                    Date = DateTime.Now,
                    Action = ActionsEnum.LocalComputersAndWhiteList,
                    State = StateEnum.InQueue
                };

                // הוסף את הטסק החדש
                tasks.Add(newTask);

                // שמור חזרה לקובץ
                File.WriteAllText(filePath, JsonSerializer.Serialize(tasks, options));
            }
            catch (Exception ex)
            {
                loggerService.Error($"Failed to write task: {ex.Message}");
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
                while (remaining.TotalSeconds > 0)
                {
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
                regionManager.RequestNavigate("ActionsRegion", "HomeUserControl");
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("task"))
            {
                MainTask = navigationContext.Parameters.GetValue<RdpTask>("task");
                TimeSpan Duration = MainTask.Date - DateTime.Now;

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
