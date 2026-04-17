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
        public IndicatorsUserControlViewModel(IPipeClientService pipeClientService)
        {
            Debug.WriteLine($"[VM CREATED] Hash={this.GetHashCode()}");

            this.pipeClientService = pipeClientService;
            this.pipeClientService.MessageReceived += OnMessageReceived;

            // Blink timers are DispatcherTimers that keep a reference to this VM. If the VM
            // is ever discarded (currently it's a singleton, but defensively we guard anyway)
            // without stopping its timers, the dictionary leaks the timer + the closure that
            // captures `this`. On app exit, also stop the timers explicitly rather than
            // relying on the dispatcher teardown to clean them up.
            var app = Application.Current;
            if (app != null)
            {
                app.Exit += OnApplicationExit;
            }
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            StopAllBlinkTimers();
        }

        private void StopAllBlinkTimers()
        {
            foreach (var kvp in _blinkTimers)
            {
                try { kvp.Value.Stop(); }
                catch (Exception ex) { Debug.WriteLine($"Error stopping blink timer '{kvp.Key}': {ex.Message}"); }
            }
            _blinkTimers.Clear();
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

            // Pipe messages arrive on a background thread. During app shutdown
            // Application.Current may be null or the Dispatcher may be shutting down.
            var app = Application.Current;
            if (app?.Dispatcher == null || app.Dispatcher.HasShutdownStarted)
                return;

            app.Dispatcher.Invoke(() =>
            {
                HandleBlink(nameof(IsAlwaysOnOpen), rdpInfoData?.IsOpenForAlwaysOnList);
                HandleBlink(nameof(IsInternalOpen), rdpInfoData?.IsOpenForLocalComputers);
                HandleBlink(nameof(IsWhiteListOpen), rdpInfoData?.IsOpenForWhiteList);
                HandleBlink(nameof(IsExternalOpen), rdpInfoData?.IsOpenForAll);
            });
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
            var app = Application.Current;
            if (app?.Dispatcher == null || app.Dispatcher.HasShutdownStarted)
                return;

            app.Dispatcher.Invoke(() =>
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
