using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeToggler.Models;
using RdpScopeToggler.Services.PipeClientService;
using RdpScopeToggler.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace RdpScopeToggler.ViewModels
{
    public class IndicatorsUserControlViewModel : BindableBase
    {
        #region Properties
        private bool isInternalOpen;
        public bool IsInternalOpen
        {
            get => isInternalOpen;
            set => SetProperty(ref isInternalOpen, value);
        }

        private bool isExternalOpen;
        public bool IsExternalOpen
        {
            get => isExternalOpen;
            set => SetProperty(ref isExternalOpen, value);
        }

        private bool isWhiteListOpen;
        public bool IsWhiteListOpen
        {
            get => isWhiteListOpen;
            set => SetProperty(ref isWhiteListOpen, value);
        }
        

        private bool isAlwaysOnOpen;
        public bool IsAlwaysOnOpen
        {
            get => isAlwaysOnOpen;
            set => SetProperty(ref isAlwaysOnOpen, value);
        }

        private readonly Dictionary<string, DispatcherTimer> _blinkTimers = new();

        #endregion

        private readonly IPipeClientService pipeClientService;
        private IRegionManager regionManager;
        public IndicatorsUserControlViewModel(IRegionManager regionManager, IPipeClientService pipeClientService)
        {
            Debug.WriteLine($"[VM CREATED] Hash={this.GetHashCode()}");

            this.regionManager = regionManager;

            this.pipeClientService = pipeClientService;
            this.pipeClientService.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(ServiceMessage message)
        {
            Debug.WriteLine("Got new message...");

            // Update the indicators
            UpdateIndicators(message.CurrentRdpState);
        }

        private void UpdateIndicators(RdpInfoData rdpInfoData)
        {
            Debug.WriteLine($"Update Indicators...");
            Application.Current.Dispatcher.Invoke(() =>
            {
                HandleBlink(nameof(IsAlwaysOnOpen), rdpInfoData?.IsOpenForAlwaysOnList);
                HandleBlink(nameof(IsInternalOpen), rdpInfoData?.IsOpenForLocalComputers);
                HandleBlink(nameof(IsWhiteListOpen), rdpInfoData?.IsOpenForWhiteList);
                HandleBlink(nameof(IsExternalOpen), rdpInfoData?.IsOpenForAll);
            });

            // Debug.WriteLine($"IsAlwaysOnOpen: {IsAlwaysOnOpen},\nIsInternalOpen: {IsInternalOpen},\nIsWhiteListOpen: {IsWhiteListOpen},\nIsExternalOpen: {IsExternalOpen}");
            // Debug.WriteLine($"IsAlwaysOnOpen: {rdpInfoData.IsOpenForAlwaysOnList},\nIsInternalOpen: {rdpInfoData.IsOpenForLocalComputers},\nIsWhiteListOpen: {rdpInfoData.IsOpenForWhiteList},\nIsExternalOpen: {rdpInfoData.IsOpenForAll}");
        }

        private void HandleBlink(string indicatorName, bool? value)
        {
            if (value == null)
            {
                if (!_blinkTimers.ContainsKey(indicatorName))
                {
                    var timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(500)
                    };

                    timer.Tick += (s, e) =>
                    {
                        bool current = GetIndicatorValue(indicatorName);
                        SetIndicatorValue(indicatorName, !current);
                    };

                    _blinkTimers[indicatorName] = timer;
                    timer.Start();
                }
            }
            else
            {
                if (_blinkTimers.TryGetValue(indicatorName, out var timer))
                {
                    timer.Stop();
                    _blinkTimers.Remove(indicatorName);
                }

                SetIndicatorValue(indicatorName, value.Value);
            }
        }

        private bool GetIndicatorValue(string name)
        {
            return name switch
            {
                nameof(IsAlwaysOnOpen) => IsAlwaysOnOpen,
                nameof(IsInternalOpen) => IsInternalOpen,
                nameof(IsWhiteListOpen) => IsWhiteListOpen,
                nameof(IsExternalOpen) => IsExternalOpen,
                _ => false
            };
        }

        private void SetIndicatorValue(string name, bool value)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                switch (name)
                {
                    case nameof(IsAlwaysOnOpen):
                        IsAlwaysOnOpen = value;
                        break;
                    case nameof(IsInternalOpen):
                        IsInternalOpen = value;
                        break;
                    case nameof(IsWhiteListOpen):
                        IsWhiteListOpen = value;
                        break;
                    case nameof(IsExternalOpen):
                        IsExternalOpen = value;
                        break;
                }
            });
        }

    }
}
