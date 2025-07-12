using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeToggler.Stores;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using RdpScopeToggler.Services.LoggerService;
using System.Windows;

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
        #endregion

        public ICommand CancelSchedulingCommand {  get; set; }

        private CancellationTokenSource _cts;
        private readonly IRegionManager regionManager;
        private TaskInfoStore taskInfoStore;
        private readonly ILoggerService loggerService;
        public WaitingUserControlViewModel(IRegionManager regionManager, TaskInfoStore taskInfoStore, ILoggerService loggerService)
        {
            this.taskInfoStore = taskInfoStore;
            this.regionManager = regionManager;
            this.loggerService = loggerService;
            CancelSchedulingCommand = new DelegateCommand(CancelScheduling);
        }

        private async void StartCountingDown()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            var remaining = taskInfoStore.Date.Value - DateTime.Now;
            if (remaining.TotalSeconds <= 0)
                return;

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
                }

                // Do action
                regionManager.RequestNavigate("ActionsRegion", "TaskUserControl");

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
            regionManager.RequestNavigate("ActionsRegion", "HomeUserControl");
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
            var date = taskInfoStore.Date.Value;
            var duration = taskInfoStore.Duration;

            string formattedDate = date.GetDateTimeFormats()[0];
            string formattedTime = $"{date.Hour:D2}:{date.Minute:D2}";

            string durationText = BuildDurationString(duration);

            string targetKey = "RemoteSystems_translator";
            if (taskInfoStore.Action == ActionsEnum.LocalComputersAndWhiteList)
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

            StartCountingDown();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
