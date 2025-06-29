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
        public WaitingUserControlViewModel(IRegionManager regionManager, TaskInfoStore taskInfoStore)
        {
            this.taskInfoStore = taskInfoStore;
            this.regionManager = regionManager;
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
                regionManager.RequestNavigate("ContentRegion", "TaskUserControl");

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
            regionManager.RequestNavigate("ContentRegion", "MainUserControl");
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var date = taskInfoStore.Date.Value;
            var duration = taskInfoStore.Duration;

            string formattedDate = date.GetDateTimeFormats()[0];
            string formattedTime = $"{date.Hour:D2}:{date.Minute:D2}";

            string durationText = BuildDurationString(duration);

            string target = "רשתות חיצוניות";
            if (taskInfoStore.Action == ActionsEnum.LocalComputersAndWhiteList)
                target = "רשימה לבנה";

            string targetMsg = $"מטרה: {target}.\r\n";
            string dateMsg = $"תאריך: {formattedDate} {formattedTime}.\r\n";
            Message = targetMsg + dateMsg;

            if (!string.IsNullOrWhiteSpace(durationText))
                Message += $"משך: {durationText}.";

            StartCountingDown();
        }

        /// <summary>
        /// Builds a duration string dynamically, omitting zero values.
        /// </summary>
        private string BuildDurationString(TimeSpan duration)
        {
            List<string> parts = new();

            if (duration.Days > 0)
                parts.Add($"{duration.Days} ימים");
            if (duration.Hours > 0)
                parts.Add($"{duration.Hours} שעות");
            if (duration.Minutes > 0)
                parts.Add($"{duration.Minutes} דקות");
            if (duration.Seconds > 0)
                parts.Add($"{duration.Seconds} שניות");

            return string.Join(" ו-", parts);
        }


        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
