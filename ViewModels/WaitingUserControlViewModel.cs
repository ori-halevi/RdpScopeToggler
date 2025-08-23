using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeCommands.Stores;
using RdpScopeToggler.Services.LoggerService;
using RdpScopeToggler.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RdpScopeToggler.ViewModels
{
    public class WaitingUserControlViewModel : BindableBase, INavigationAware
    {
        #region Properties
        private int countDownDay;
        public int CountDownDay
        {
            get { return countDownDay; }
            set
            {
                SetProperty(ref countDownDay, value);
            }
        }
        private int countDownHour;
        public int CountDownHour
        {
            get { return countDownHour; }
            set
            {
                SetProperty(ref countDownHour, value);
            }
        }

        private int countDownMinute;
        public int CountDownMinute
        {
            get { return countDownMinute; }
            set
            {
                SetProperty(ref countDownMinute, value);
            }
        }
        private int countDownSecond;
        public int CountDownSecond
        {
            get { return countDownSecond; }
            set
            {
                SetProperty(ref countDownSecond, value);
            }
        }
        private string message;
        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }

        private RdpTask MainTask { get; set; }
        private RdpTask CloseRdpTask { get; set; }
        #endregion

        public ICommand CancelSchedulingCommand {  get; set; }

        private CancellationTokenSource _cts;
        private readonly IRegionManager regionManager;
        private TaskInfoStore taskInfoStore;
        private readonly ILoggerService loggerService;
        private bool _isCountingDown = false;
        public WaitingUserControlViewModel(IRegionManager regionManager, TaskInfoStore taskInfoStore, ILoggerService loggerService)
        {
            this.taskInfoStore = taskInfoStore;
            this.regionManager = regionManager;
            this.loggerService = loggerService;
            CancelSchedulingCommand = new DelegateCommand(CancelScheduling);
        }

        private async void StartCountingDown(RdpTask task)
        {
            if (_isCountingDown) return; // אם כבר רץ – לא להריץ שוב
            _isCountingDown = true;
            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();

                Debug.WriteLine(taskInfoStore.Date);

                var remaining = task.Date - DateTime.Now;

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
                // Clean always
                _cts?.Dispose();
                _cts = null;
                _isCountingDown = false; // שחרור הנעילה
                //regionManager.RequestNavigate("ActionsRegion", "HomeUserControl");
            }
        }



        private void CancelScheduling()
        {
            if (_cts != null)
            {
                _cts.Cancel();     // מבטל את המשימה הרצה
                _cts.Dispose();    // משחרר משאבים
                _cts = null;       // מוחק את ההתייחסות
            }
            CancelTasks();
            regionManager.RequestNavigate("ActionsRegion", "HomeUserControl");
        }

        private void CancelTasks()
        {
            RdpTask[] rdpTasks = new RdpTask[]
            {
                MainTask,
                CloseRdpTask
            };
            WriteToFile(rdpTasks);
        }
        private void WriteToFile(RdpTask[] rdpTasks)
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
                    if (task.Id.Equals(rdpTasks[0].Id) || task.Id.Equals(rdpTasks[1].Id))
                    {
                        task.State = StateEnum.Canceled;
                    }
                }

                // שמור חזרה לקובץ
                File.WriteAllText(filePath, JsonSerializer.Serialize(tasks, options));
            }
            catch (Exception ex)
            {
                loggerService.Error($"Failed to write task: {ex.Message}");
            }
        }


        private static string T(string key)
        {
            var result = Application.Current.TryFindResource(key) as string;
            if (result == null)
            {
                // אפשר גם לכתוב לוג
                Console.WriteLine($"Missing translation key: {key}");
                return key;
            }
            return result;
        }


        /// <summary>
        /// Builds a duration string dynamically, omitting zero values.
        /// </summary>
        private string BuildDurationString(TimeSpan duration)
        {
            List<string> parts = new();

            if (duration.Days > 0)
                parts.Add($"{duration.Days} {T("Days_translator")}");
            if (duration.Hours > 0)
                parts.Add($"{duration.Hours} {T("Hours_translator")}");
            if (duration.Minutes > 0)
                parts.Add($"{duration.Minutes} {T("Minutes_translator")}");
            if (duration.Seconds > 0)
                parts.Add($"{duration.Seconds} {T("Seconds_translator")}");

            return string.Join($" {T("And_translator")}-", parts);
        }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("task"))
            {
                MainTask = navigationContext.Parameters.GetValue<RdpTask>("task");
                CloseRdpTask = MainTask.NextTask;
                TimeSpan Duration = MainTask.Date - DateTime.Now;

                CountDownDay = Duration.Days;
                CountDownHour = Duration.Hours;
                CountDownMinute = Duration.Minutes;
                CountDownSecond = Duration.Seconds;

                var date = MainTask.Date;
                TimeSpan duration = CloseRdpTask.Date - MainTask.Date;

                string formattedDate = date.GetDateTimeFormats()[0];
                string formattedTime = $"{date.Hour:D2}:{date.Minute:D2}";

                string durationText = BuildDurationString(duration);

                string targetKey = "RemoteSystems_translator";
                if (MainTask.Action == ActionsEnum.LocalComputersAndWhiteList)
                    targetKey = "WhiteList_translator";

                string target = T(targetKey);

                string targetMsg = $"{T("Target_translator")}: {target}.\r\n";
                string dateMsg = $"{T("Date_translator")}: {formattedDate} {formattedTime}.\r\n";
                Message = targetMsg + dateMsg;

                if (!string.IsNullOrWhiteSpace(durationText))
                    Message += $"{T("Duration_translator")}: {durationText}.";

                // לוג ברור ומפורט
                loggerService.Info(
                    $"Scheduled RDP accessibility configured. Target: {target}. " +
                    $"Start date: {formattedDate} {formattedTime}. " +
                    $"Duration: {durationText}."
                );

                StartCountingDown(MainTask);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
