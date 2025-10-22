using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeToggler.Enums;
using RdpScopeToggler.Helpers;
using RdpScopeToggler.Models;
using RdpScopeToggler.Services.LoggerService;
using RdpScopeToggler.Services.PipeClientService;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        private RdpTask NextTask { get; set; }

        #endregion

        private CancellationTokenSource _cts;
        private readonly ILoggerService loggerService;
        private readonly IPipeClientService pipeClientService;
        private bool _isCountingDown = false;

        public ICommand CancelSchedulingCommand {  get; set; }

        public WaitingUserControlViewModel(ILoggerService loggerService, IPipeClientService pipeClientService)
        {
            this.loggerService = loggerService;
            this.pipeClientService = pipeClientService;

            CancelSchedulingCommand = new DelegateCommand(CancelScheduling);
        }

        private async void StartCountingDown()
        {
            if (_isCountingDown) return; // If already counting down - don't count down again
            _isCountingDown = true;

            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();

                // Calculate the remaining time of the main task
                TimeSpan timeLeft = MainTask.Date - DateTime.Now;

                while (timeLeft.TotalSeconds > 0)
                {
                    if (_cts == null) break;

                    await Task.Delay(1000, _cts.Token);
                    timeLeft = timeLeft.Subtract(TimeSpan.FromSeconds(1));

                    // Update the UI
                    CountDownDay = timeLeft.Days;
                    CountDownHour = timeLeft.Hours;
                    CountDownMinute = timeLeft.Minutes;
                    CountDownSecond = timeLeft.Seconds;
                }
            }
            catch (TaskCanceledException)
            {
                // Counting down was canceled
            }
            finally
            {
                if (_cts != null)
                {
                    _cts.Dispose();    // Release resources
                    _cts = null;       // Delete the reference
                }
                _isCountingDown = false; // Reset
            }
        }

        /// <summary>
        /// Cancel the running timer and send the remove task command
        /// </summary>
        private void CancelScheduling()
        {
            // Cancel the running task
            if (_cts != null)
            {
                _cts.Cancel();     // Cancel the running task
                _cts.Dispose();    // Release resources
                _cts = null;       // Delete the reference
            }

            // Send the remove task command to the service
            pipeClientService.SendRemoveTask(MainTask.Id);
        }

        /// <summary>
        /// Builds a duration string dynamically, omitting zero values.
        /// </summary>
        private string BuildDurationString(TimeSpan duration)
        {
            List<string> parts = new();

            if (duration.Days > 0)
                parts.Add($"{duration.Days} {TranslationHelper.Translate("Days_translator")}");
            if (duration.Hours > 0)
                parts.Add($"{duration.Hours} {TranslationHelper.Translate("Hours_translator")}");
            if (duration.Minutes > 0)
                parts.Add($"{duration.Minutes} {TranslationHelper.Translate("Minutes_translator")}");
            if (duration.Seconds > 0)
                parts.Add($"{duration.Seconds} {TranslationHelper.Translate("Seconds_translator")}");

            return string.Join($" {TranslationHelper.Translate("And_translator")}-", parts);
        }



        private void BuildDisplayMessage()
        {
            string formattedDate = MainTask.Date.GetDateTimeFormats()[0];
            string formattedTime = $"{MainTask.Date.Hour:D2}:{MainTask.Date.Minute:D2}";


            string durationText = BuildDurationString(NextTask.Date - MainTask.Date);

            string targetKey = "AllAddresses_translator";
            if (MainTask.Action == ActionsEnum.LocalComputersAndWhiteList)
                targetKey = "WhiteList_translator";

            string target = TranslationHelper.Translate(targetKey);

            string targetMsg = $"{TranslationHelper.Translate("Target_translator")}: {target}.\r\n";
            string dateMsg = $"{TranslationHelper.Translate("Date_translator")}: {formattedDate} {formattedTime}.\r\n";
            Message = targetMsg + dateMsg;

            if (!string.IsNullOrWhiteSpace(durationText))
                Message += $"{TranslationHelper.Translate("Duration_translator")}: {durationText}.";


            // לוג ברור ומפורט
            loggerService.Info(
                $"Scheduled RDP accessibility configured. Target: {target}. " +
                $"Start date: {formattedDate} {formattedTime}. " +
                $"Duration: {durationText}."
            );
        }

        /// <summary>
        /// Initialize the clock
        /// </summary>
        private void InitializeClock()
        {
            // Calculate the remaining time of the main task
            TimeSpan timeLeft = MainTask.Date - DateTime.Now;

            // Initialize the clock
            CountDownDay = timeLeft.Days;
            CountDownHour = timeLeft.Hours;
            CountDownMinute = timeLeft.Minutes;
            CountDownSecond = timeLeft.Seconds;
        }

        #region Navigation Methods

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;

            if (!parameters.ContainsKey("task"))
                return;
            
            // Initialize the Tasks
            MainTask = parameters.GetValue<RdpTask>("task");
            NextTask = MainTask.NextTask;

            if (MainTask == null) return;
            if (NextTask == null) return;

            // Initialize the clock on the GUI
            InitializeClock();

            // Build the message
            BuildDisplayMessage();

            // Start the countdown
            StartCountingDown();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        #endregion
    }
}
