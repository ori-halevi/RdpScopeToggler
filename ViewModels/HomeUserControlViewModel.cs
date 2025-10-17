using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeToggler.Helpers;
using RdpScopeToggler.Models;
using RdpScopeToggler.Services.LoggerService;
using RdpScopeToggler.Services.PipeClientService;
using RdpScopeToggler.Stores;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace RdpScopeToggler.ViewModels
{
    public class HomeUserControlViewModel : BindableBase, INavigationAware
    {
        #region Properties

        private TimeSpan Duration;

        private int countDownDay;
        public int CountDownDay
        {
            get { return countDownDay; }
            set
            {
                SetProperty(ref countDownDay, value);
                int days = value;
                int hours = Duration.Hours;
                int minutes = Duration.Minutes;
                int seconds = Duration.Seconds;
                Duration = new(days, hours, minutes, seconds);
            }
        }

        private int countDownHour;
        public int CountDownHour
        {
            get { return countDownHour; }
            set
            {
                SetProperty(ref countDownHour, value);
                int days = Duration.Days;
                int hours = value;
                int minutes = Duration.Minutes;
                int seconds = Duration.Seconds;
                Duration = new(days, hours, minutes, seconds);
            }
        }

        private int countDownMinute;
        public int CountDownMinute
        {
            get { return countDownMinute; }
            set
            {
                SetProperty(ref countDownMinute, value);
                int days = Duration.Days;
                int hours = Duration.Hours;
                int minutes = value;
                int seconds = Duration.Seconds;
                Duration = new(days, hours, minutes, seconds);
            }
        }

        private int countDownSecond;
        public int CountDownSecond
        {
            get { return countDownSecond; }
            set
            {
                SetProperty(ref countDownSecond, value);
                int days = Duration.Days;
                int hours = Duration.Hours;
                int minutes = Duration.Minutes;
                int seconds = value;
                Duration = new(days, hours, minutes, seconds);
            }
        }

        private string dateTimeError;
        public string DateTimeError
        {
            get => dateTimeError;
            set
            {
                if (SetProperty(ref dateTimeError, value))
                    RaisePropertyChanged(nameof(HasDateTimeError)); // notify UI HasDateTimeError changed too
            }
        }

        public bool HasDateTimeError => !string.IsNullOrEmpty(DateTimeError);

        private DateTime? selectedDate;
        public DateTime? SelectedDate
        {
            get => selectedDate;
            set
            {
                SetProperty(ref selectedDate, value);
                UpdateCombinedDateTime();
            }
        }

        private DateTime? selectedTime;
        public DateTime? SelectedTime
        {
            get => selectedTime;
            set
            {
                SetProperty(ref selectedTime, value);
                UpdateCombinedDateTime();
            }
        }

        private DateTime selectedDateTime;
        public DateTime SelectedDateTime
        {
            get => selectedDateTime;
            private set
            {
                SetProperty(ref selectedDateTime, value);
                ValidateSelectedDateTime();
            }
        }


        private bool isDateValid = false;
        public bool IsDateValid
        {
            get { return isDateValid; }
            set { SetProperty(ref isDateValid, value); }
        }


        private bool _isDateTimeEnabled;
        public bool IsDateTimeEnabled
        {
            get => _isDateTimeEnabled;
            set
            {
                if (SetProperty(ref _isDateTimeEnabled, value))
                    ValidateSelectedDateTime();
                if (!value)
                    IsDateValid = true;
            }
        }


        private bool _isMoreOptionsVisible;
        public bool IsMoreOptionsVisible
        {
            get => _isMoreOptionsVisible;
            set => SetProperty(ref _isMoreOptionsVisible, value);
        }


        private ActionsEnum selectedAction;
        public ActionsEnum SelectedAction
        {
            get => selectedAction;
            set
            {
                SetProperty(ref selectedAction, value);
                Debug.WriteLine($"SelectedAction changed to: {value}");
            }
        }

        public ICommand OpenAccessCommand { get; }
        public ICommand UpdateDateCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        public ObservableCollection<ActionsEnum> Options { get; }

        #endregion

        private IRegionManager regionManager;
        private readonly ILoggerService loggerService;
        private readonly IPipeClientService pipeClientService;

        public HomeUserControlViewModel(IRegionManager regionManager, ILoggerService loggerService, IPipeClientService pipeClientService)
        {
            this.pipeClientService = pipeClientService;
            this.loggerService = loggerService;
            this.regionManager = regionManager;

            CountDownDay = 0;
            CountDownHour = 0;
            CountDownMinute = 1;
            CountDownSecond = 0;
            var now = DateTime.Now;
            now.AddMinutes(2);
            SelectedDateTime = now;
            SelectedDate = now.Date;
            SelectedTime = DateTime.Now.AddMinutes(2);

            IsDateValid = false;
            IsDateTimeEnabled = false;
            Options =
            [
                ActionsEnum.RemoteSystems,
                ActionsEnum.WhiteList
            ];
            SelectedAction = Options[0];

            #region Commands

            OpenAccessCommand = new DelegateCommand(OpenAccess);
            UpdateDateCommand = new DelegateCommand(UpdateDate);
            NavigateToSettingsCommand = new DelegateCommand(NavigateToSettings);

            #endregion
        }


        private void OpenAccess()
        {
            RdpTask task = new();
            task.Action = SelectedAction;

            if (IsDateTimeEnabled)
            {
                ValidateSelectedDateTime();
                if (IsDateTimeEnabled && SelectedDateTime < DateTime.Now.AddMinutes(1)) return;
                task.Date = SelectedDateTime;
                DateTime closeRdpDate = SelectedDateTime.Add(Duration);
                task.NextTask = new RdpTask(closeRdpDate, ActionsEnum.CloseRdp);
                pipeClientService.SendAddTask(task);
                return;
            }

            DateTime now = DateTime.Now;
            DateTime closeRdpDate1 = now.Add(Duration);
            task.Date = now;
            task.NextTask = new RdpTask(closeRdpDate1, ActionsEnum.CloseRdp);
            pipeClientService.SendAddTask(task);
        }

        private void UpdateDate()
        {
            var now = DateTime.Now;
            now.AddMinutes(2);
            SelectedDateTime = now;
            SelectedDate = now.Date;
            SelectedTime = DateTime.Now.AddMinutes(2);
        }

        private void NavigateToSettings()
        {
            regionManager.RequestNavigate("ContentRegion", "SettingsUserControl");
        }


        private void ValidateSelectedDateTime()
        {
            IsDateValid = false;
            if (IsDateTimeEnabled && SelectedDateTime < DateTime.Now.AddMinutes(1))
            {
                DateTimeError = TranslationHelper.Translate("FutureDateAtLeastOneMinute_translator");
            }
            else
            {
                DateTimeError = null;
                IsDateValid = true;
            }
        }

        private void UpdateCombinedDateTime()
        {
            if (SelectedDate is DateTime date && SelectedTime is DateTime time)
            {
                SelectedDateTime = date.Date + time.TimeOfDay;
            }
        }


        #region Navigation Methods

        public void OnNavigatedTo(NavigationContext navigationContext) { }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        #endregion
    }
}
