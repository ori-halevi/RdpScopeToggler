using Prism.Mvvm;
using Prism.Commands;
using System.Windows.Input;
using Prism.Navigation.Regions;
using System;
using System.Collections.ObjectModel;
using RdpScopeToggler.Stores;
using System.Windows;
using System.Diagnostics;


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

        private DateTime selectedDateTime;
        public DateTime SelectedDateTime
        {
            get => selectedDateTime;
            set
            {
                SetProperty(ref selectedDateTime, value);
                taskInfoStore.Date = value;
            }
        }
        private bool _isDateTimeEnabled;
        public bool IsDateTimeEnabled
        {
            get => _isDateTimeEnabled;
            set => SetProperty(ref _isDateTimeEnabled, value);
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



            StartCommand = new DelegateCommand(() =>
            {
                if (IsDateTimeEnabled)
                {
                    regionManager.RequestNavigate("ContentRegion", "WaitingUserControl");
                    return;
                }
                regionManager.RequestNavigate("ContentRegion", "TaskUserControl");
            });


        }

        public ICommand StartCommand { get; }

        public void OnNavigatedTo(NavigationContext navigationContext) { }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
