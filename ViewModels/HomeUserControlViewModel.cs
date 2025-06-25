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
using RdpScopeToggler.Services.NotificationService;


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

        private DateTime selectedDateTime;
        public DateTime SelectedDateTime
        {
            get => selectedDateTime;
            set
            {
                SetProperty(ref selectedDateTime, value);
                taskInfoStore.Date = value;
                ValidateSelectedDateTime();
            }
        }

        private void ValidateSelectedDateTime()
        {
            if (IsDateTimeEnabled && SelectedDateTime < DateTime.Now.AddMinutes(1))
                DateTimeError = "בחר תאריך עתידי לפחות בדקה אחת";
            else
                DateTimeError = null;
        }


        private bool _isDateTimeEnabled;
        public bool IsDateTimeEnabled
        {
            get => _isDateTimeEnabled;
            set
            {
                if (SetProperty(ref _isDateTimeEnabled, value))
                    ValidateSelectedDateTime();
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
                    taskInfoStore.Action = ActionsEnum.WhiteList;
                }
            }
        }
        public ICommand StartCommand { get; }
        public ICommand OpenSettingsWindowCommand { get; }


        public ObservableCollection<string> Options { get; }
        #endregion

        private TaskInfoStore taskInfoStore;

        public HomeUserControlViewModel(IRegionManager regionManager, TaskInfoStore taskInfoStore)
        {
            this.taskInfoStore = taskInfoStore;

            CountDownDay = 00;
            CountDownHour = 00;
            CountDownMinute = 00;
            CountDownSecond = 00;
            SelectedDateTime = DateTime.Now;
            IsDateTimeEnabled = false;
            Options = new ObservableCollection<string>
            {
                "Remote systems",
                "White list"
            };
            SelectedAction = Options[0];


            OpenSettingsWindowCommand = new DelegateCommand(OpenSettingsWindow);
            StartCommand = new DelegateCommand(() =>
            {
                if (IsDateTimeEnabled)
                {
                    // Add validation
                    regionManager.RequestNavigate("ContentRegion", "WaitingUserControl");
                    return;
                }
                regionManager.RequestNavigate("ContentRegion", "TaskUserControl");
            });


        }

        private void OpenSettingsWindow()
        {
            var settingsWindow = new SettingsWindow();

            // אפשרות לחיבור DataContext אם צריך ViewModel ייעודי
            // settingsWindow.DataContext = new SettingsWindowViewModel();

            // פתיחה כחלון מודאלי
            settingsWindow.Owner = Application.Current.MainWindow;
            settingsWindow.ShowDialog();
        }


        public void OnNavigatedTo(NavigationContext navigationContext) { }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
