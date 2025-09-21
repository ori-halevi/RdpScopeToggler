using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeToggler.Models;
using RdpScopeToggler.Services.LoggerService;
using RdpScopeToggler.Services.PipeClientService;
using System;
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

        public ICommand SkipCountdownCommand { get; set; }

        #endregion

        private CancellationTokenSource _cts;
        private readonly ILoggerService loggerService;
        private readonly IPipeClientService pipeClientService;
        private bool _isCountingDown = false;

        public TaskUserControlViewModel(ILoggerService loggerService, IPipeClientService pipeClientService)
        {
            this.pipeClientService = pipeClientService;
            this.loggerService = loggerService;

            SkipCountdownCommand = new DelegateCommand(SkipCountdown);
        }

        /// <summary>
        /// Cancel the running timer and send the remove task command and create the end result task
        /// </summary>
        private void SkipCountdown()
        {
            if (_cts != null)
            {
                _cts.Cancel();     // Cancel the running task
                _cts.Dispose();    // Release resources
                _cts = null;       // Delete the reference
            }

            // Send the remove task command to the service
            pipeClientService.SendRemoveTask(MainTask.Id);

            // Create the end result task
            RdpTask endResultTask = new (DateTime.Now, MainTask.Action);

            // Send the add task command to the service
            pipeClientService.SendAddTask(endResultTask);
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

            // Initialize the Task
            MainTask = parameters.GetValue<RdpTask>("task");

            if (MainTask == null) return;

            // Update the MainTask to the right task
            if (MainTask.State == StateEnum.Executed && MainTask.NextTask != null)
            {
                MainTask = MainTask.NextTask;
            }

            // Initialize the clock on the GUI
            InitializeClock();


            var durationString = $"{CountDownDay} days, {CountDownHour} hours, {CountDownMinute} minutes, {CountDownSecond} seconds";
            loggerService.Info($"RDP accessibility window started. The RDP will be available for: {durationString}");

            StartCountingDown();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        #endregion
    }
}
