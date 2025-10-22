using Prism.Navigation.Regions;
using RdpScopeToggler.Models;
using RdpScopeToggler.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RdpScopeToggler.Services.PipeClientService
{
    public class PipeClientService : IPipeClientService
    {
        private readonly string _pipeName;
        private NamedPipeClientStream? _client;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private bool _isListening = false;
        private IRegionManager _regionManager;
        private bool _isConnected = false;
        private readonly object _lock = new();

        public CancellationTokenSource Cts { get; set; } = new();

        public event Action<ServiceMessage>? MessageReceived;
        public event Action<List<Client>>? WhiteListReceived;
        public event Action<List<Client>>? AlwaysTrustedListReceived;

        public bool IsConnected => _client?.IsConnected ?? false;

        public PipeClientService(IRegionManager regionManager)
        {
            _pipeName = "RdpScopePipe";
            _regionManager = regionManager;

            Cts = new CancellationTokenSource();
        }

        public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (_isListening || _isConnected)
                return true;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                    await _client.ConnectAsync(2000, cancellationToken);
                    _reader = new StreamReader(_client, Encoding.UTF8);
                    _writer = new StreamWriter(_client, Encoding.UTF8) { AutoFlush = true };

                    _isConnected = true;

                    // Start listening loop asynchronously
                    _ = Task.Run(() => ListenLoop(cancellationToken), cancellationToken);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Pipe connect failed: {ex.Message}");
                }

                try { await Task.Delay(2000, cancellationToken); }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine("Pipe connect cancelled.");
                    break;
                }
            }

            Cleanup();
            return false;
        }

        private async Task ListenLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _client != null && _isConnected)
                {
                    if (!_client.IsConnected)
                    {
                        Debug.WriteLine("Pipe not connected, exiting ListenLoop");
                        break;
                    }

                    string? line = null;
                    try
                    {
                        _isListening = true;
                        line = await _reader!.ReadLineAsync(cancellationToken);
                        if (line == null) throw new IOException("Pipe closed by server");

                        var message = JsonSerializer.Deserialize<ServiceMessage>(line, new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Converters = { new JsonStringEnumConverter() }
                        });

                        // Fire events if message exists
                        MessageReceived?.Invoke(message);
                        if (message?.WhiteList != null) WhiteListReceived?.Invoke(message.WhiteList);
                        if (message?.AlwaysTrustedList != null) AlwaysTrustedListReceived?.Invoke(message.AlwaysTrustedList);

                        if (message?.Error != null)
                            Debug.WriteLine($"Pipe error received: {message.Error}");
                    }
                    catch (ObjectDisposedException)
                    {
                        Debug.WriteLine("Pipe disposed, handling disconnection immediately");
                        break; // exit loop to cleanup and reconnect
                    }
                    catch (IOException)
                    {
                        Debug.WriteLine("Pipe I/O error, handling disconnection immediately");
                        break; // exit loop to cleanup and reconnect
                    }
                }
            }
            finally
            {
                // 1. Cleanup resources
                Cleanup();

                // 2. Start reconnect loop until the service is back
                await TryReconnectLoop();
            }
        }

        private async Task TryReconnectLoop()
        {
            // ⚠️ Ensure navigation runs on the UI thread
            var app = Application.Current;
            if (app?.Dispatcher != null && app.Dispatcher.HasShutdownStarted == false)
            {
                app.Dispatcher.Invoke(() =>
                {
                    _regionManager.RequestNavigate("ActionsRegion", "HomeUserControl");
                    _regionManager.RequestNavigate("ContentRegion", "WaitingForServiceUserControl");
                });
            }

            while (!_isConnected)
            {
                try
                {

                    Debug.WriteLine("Attempting to reconnect to service...");

                    Cts = new CancellationTokenSource();
                    if (await ConnectAsync(Cts.Token))
                    {
                    }
                    else
                    {
                        // אופציונלי: תראה הודעת שגיאה/תנסה שוב
                        Debug.WriteLine("Couldn't connect to the server.");
                    }

                    if (_isConnected)
                    {
                        Debug.WriteLine("Reconnected to service.");

                        // ⚠️ Ensure navigation runs on the UI thread
                        if (app?.Dispatcher != null && app.Dispatcher.HasShutdownStarted == false)
                        {
                            app.Dispatcher.Invoke(() =>
                            {
                                _regionManager.RequestNavigate("ActionsRegion", "HomeUserControl");
                                _regionManager.RequestNavigate("ContentRegion", "WaitingForServiceUserControl");
                            });
                        }

                        await AskForUpdate();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Reconnect attempt failed: {ex.Message}");
                }
            }
        }


        private void Cleanup()
        {
            try { _reader?.Dispose(); }
            catch (Exception ex) { Debug.WriteLine($"Error disposing pipe reader: {ex.Message}"); }
            try { _writer?.Dispose(); }
            catch (Exception ex) { Debug.WriteLine($"Error disposing pipe writer: {ex.Message}"); }
            try { _client?.Dispose(); }
            catch (Exception ex) { Debug.WriteLine($"Error disposing pipe client: {ex.Message}"); }
            _reader = null;
            _writer = null;
            _client = null;
            _isConnected = false;
            _isListening = false;
        }

        public async Task SendAsync(string message, CancellationToken cancellationToken = default)
        {
            if (!IsConnected) return;

            try
            {
                await _writer!.WriteLineAsync(message);
                await _writer.FlushAsync();
            }
            catch (ObjectDisposedException)
            {
                _isConnected = false;
            }
            catch (IOException)
            {
                _isConnected = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SendAsync error: {ex.Message}");
            }
        }

        public void SendAddTask(RdpTask taskToAdd)
        {
            var payload = new { command = "AddTask", task = taskToAdd };
            _ = SafeFireAndForget(SendToWindowsServer(payload));
        }

        public void SendRemoveTask(string taskId)
        {
            var payload = new { command = "UpdateTaskAsCanceled", taskId };
            _ = SafeFireAndForget(SendToWindowsServer(payload));
        }

        // ⚠️ IMPORTANT: Do NOT dispose the main _client here! Open a new pipe if needed for synchronous call
        public async Task<RdpTask?> GetUpcomingTaskAsync()
        {
            var payload = new { command = "GetUpcomingTask" };
            string json = JsonSerializer.Serialize(payload);

            try
            {
                using var tempPipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                await tempPipe.ConnectAsync(2000);
                using var writer = new StreamWriter(tempPipe) { AutoFlush = true };
                using var reader = new StreamReader(tempPipe, Encoding.UTF8);

                await writer.WriteLineAsync(json);
                string? responseJson = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(responseJson)) return null;

                return JsonSerializer.Deserialize<RdpTask>(responseJson, new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetUpcomingTaskAsync failed: {ex.Message}");
                return null;
            }
        }


        public Task AskForUpdate() =>
            SafeFireAndForget(SendToWindowsServer(new { command = "Update" }));

        public Task SendUpdateAlwaysTrustedList(List<Client> clients) =>
            SafeFireAndForget(SendToWindowsServer(new { command = "UpdateAlwaysTrustedList", alwaysTrustedList = clients }));

        public Task SendUpdateWhiteList(List<Client> clients) =>
            SafeFireAndForget(SendToWindowsServer(new { command = "UpdateWhiteList", whiteList = clients }));

        public Task AskWhiteListUpdate() =>
            SafeFireAndForget(SendToWindowsServer(new { command = "GetWhiteList" }));

        public Task AskAlwaysTrustedListUpdate() =>
            SafeFireAndForget(SendToWindowsServer(new { command = "GetAlwaysTrustedList" }));

        private async Task SendToWindowsServer(object payload)
        {
            try
            {
                if (!_isConnected || _writer == null)
                    await ConnectAsync();

                if (_writer == null) return;

                string json = JsonSerializer.Serialize(payload);

                lock (_lock)
                {
                    try
                    {
                        _writer.WriteLine(json);
                    }
                    catch (ObjectDisposedException)
                    {
                        Debug.WriteLine("Pipe closed while sending message");
                        _isConnected = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SendToWindowsServer failed: {ex.Message}");
            }
        }

        // Helper to safely run fire-and-forget tasks
        private Task SafeFireAndForget(Task task)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unobserved task exception: {ex.Message}");
                }
            });
        }
    }
}
