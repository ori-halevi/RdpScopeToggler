using Prism.Mvvm;
using Prism.Commands;
using System.Windows.Input;
using Prism.Navigation.Regions;
using System;
using System.Collections.ObjectModel;
using RdpScopeToggler.Stores;
using System.Windows;
using System.Diagnostics;
using RdpScopeToggler.Views;


namespace RdpScopeToggler.ViewModels
{
    public class HomeUserControlViewModel : BindableBase, INavigationAware
    {
        #region Properties
        private int countDownDay;
        public int CountDownDay
        {
            get { return countDownDay; }
            set
            {
                SetProperty(ref countDownDay, value);
                int days = value;
                int hours = taskInfoStore.Duration.Hours;
                int minutes = taskInfoStore.Duration.Minutes;
                int seconds = taskInfoStore.Duration.Seconds;
                TimeSpan duration = new(days, hours, minutes, seconds);
                taskInfoStore.Duration = duration;
            }
        }
        private int countDownHour;
        public int CountDownHour
        {
            get { return countDownHour; }
            set
            {
                SetProperty(ref countDownHour, value);
                int days = taskInfoStore.Duration.Days;
                int hours = value;
                int minutes = taskInfoStore.Duration.Minutes;
                int seconds = taskInfoStore.Duration.Seconds;
                TimeSpan duration = new(days, hours, minutes, seconds);
                taskInfoStore.Duration = duration;
            }
        }

        private int countDownMinute;
        public int CountDownMinute
        {
            get { return countDownMinute; }
            set
            {
                SetProperty(ref countDownMinute, value);
                int days = taskInfoStore.Duration.Days;
                int hours = taskInfoStore.Duration.Hours;
                int minutes = value;
                int seconds = taskInfoStore.Duration.Seconds;
                TimeSpan duration = new(days, hours, minutes, seconds);
                taskInfoStore.Duration = duration;
            }
        }
        private int countDownSecond;
        public int CountDownSecond
        {
            get { return countDownSecond; }
            set
            {
                SetProperty(ref countDownSecond, value);
                int days = taskInfoStore.Duration.Days;
                int hours = taskInfoStore.Duration.Hours;
                int minutes = taskInfoStore.Duration.Minutes;
                int seconds = value;
                TimeSpan duration = new(days, hours, minutes, seconds);
                taskInfoStore.Duration = duration;
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
                taskInfoStore.Date = value;
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


        private string selectedAction;
        public string SelectedAction
        {
            get => selectedAction;
            set
            {
                SetProperty(ref selectedAction, value);
                Debug.WriteLine($"SelectedAction changed to: {value}");
                if (value == Options[0])
                {
                    taskInfoStore.Action = ActionsEnum.RemoteSystems;
                }
                else
                {
                    taskInfoStore.Action = ActionsEnum.LocalComputersAndWhiteList;
                }
            }
        }

        public ICommand StartCommand { get; }
        public ICommand UpdateDateCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }


        public ObservableCollection<string> Options { get; }
        #endregion

        private TaskInfoStore taskInfoStore;
        private IRegionManager regionManager;

        public HomeUserControlViewModel(IRegionManager regionManager, TaskInfoStore taskInfoStore)
        {
            this.regionManager = regionManager;
            this.taskInfoStore = taskInfoStore;

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
            Options = new ObservableCollection<string>
            {
                "Remote systems",
                "White list"
            };
            SelectedAction = Options[0];


            NavigateToSettingsCommand = new DelegateCommand(NavigateToSettings);
            StartCommand = new DelegateCommand(() =>
            {
                if (IsDateTimeEnabled)
                {
                    ValidateSelectedDateTime();
                    if (IsDateTimeEnabled && SelectedDateTime < DateTime.Now.AddMinutes(1)) return;
                    regionManager.RequestNavigate("ContentRegion", "WaitingUserControl");
                    return;
                }
                regionManager.RequestNavigate("ContentRegion", "TaskUserControl");
            });

            UpdateDateCommand = new DelegateCommand(() =>
            {
                var now = DateTime.Now;
                now.AddMinutes(2);
                SelectedDateTime = now;
                SelectedDate = now.Date;
                SelectedTime = DateTime.Now.AddMinutes(2);
            });

        }

        private void ValidateSelectedDateTime()
        {
            IsDateValid = false;
            if (IsDateTimeEnabled && SelectedDateTime < DateTime.Now.AddMinutes(1))
            {
                DateTimeError = "בחר תאריך עתידי לפחות בדקה אחת";
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


        private void NavigateToSettings()
        {
            regionManager.RequestNavigate("ContentRegion", "SettingsUserControl");
        }

        public void OnNavigatedTo(NavigationContext navigationContext) { }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
