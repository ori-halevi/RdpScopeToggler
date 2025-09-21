using Prism.Navigation.Regions;
using RdpScopeCommands.Stores;
using RdpScopeToggler.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.PipeClientService
{
    public class PipeClientService : IPipeClientService
    {
        private readonly string _pipeName;
        private NamedPipeClientStream _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        private bool _isListening = false;
        private CancellationTokenSource? _cts;
        private IRegionManager regionManager;
        private bool _isConnected = false;
        private readonly object _lock = new();

        public event Action<ServiceMessage> MessageReceived;
        public event Action? Connected;
        public event Action? Disconnected;
        public bool IsConnected => _client?.IsConnected ?? false;

        public PipeClientService(IRegionManager regionManager)
        {
            _pipeName = "RdpScopePipe";
            this.regionManager = regionManager;
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

                    Connected?.Invoke();
                    _isConnected = true;
                    _isListening = true;

                    // ListenLoop will wait until the connection will be closed, not call ConnectAsync again
                    _ = Task.Run(() => ListenLoop(cancellationToken), cancellationToken);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Pipe connect failed: {ex.Message}");
                }

                try { await Task.Delay(2000, cancellationToken); }
                catch (TaskCanceledException) { return false; }
            }

            return false;
        }

        private async Task ListenLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _client != null && _client.IsConnected)
                {
                    try
                    {
                        string? line = await _reader.ReadLineAsync();
                        if (line == null) break;

                        var messageAsJson = JsonSerializer.Deserialize<ServiceMessage>(line, new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Converters = { new JsonStringEnumConverter() }
                        });

                        MessageReceived?.Invoke(messageAsJson);
                        Debug.WriteLine($"Pipe message received: {line}");
                    }
                    catch (Exception ex) when (ex is TimeoutException || ex is IOException)
                    {
                        Debug.WriteLine($"Pipe read error: {ex.Message}");
                    }
                }
            }
            finally
            {
                Disconnected?.Invoke();
                Cleanup();
            }
        }



        private void Cleanup()
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _client?.Dispose();
            _reader = null;
            _writer = null;
            _client = null;
            _isConnected = false;
            _isListening = false;
        }









        public Task AskForUpdate()
        {
            var message = new
            {
                command = "Update"
            };
            SendToWindowsServer(message);
            return Task.CompletedTask;
        }
        public async Task SendAsync(string message, CancellationToken cancellationToken = default)
        {
            if (!IsConnected) return;

            try
            {
                await _writer.WriteLineAsync(message); // שולח עם \n
                await _writer.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PipeClientService SendAsync error: {ex.Message}");
            }
        }

        public void SendAddTask(RdpTask taskToAdd)
        {
            var payload = new
            {
                command = "AddTask",
                task = taskToAdd
            };

            SendToWindowsServer(payload);
        }

        public void SendRemoveTask(string taskId)
        {
            var payload = new
            {
                command = "UpdateTaskAsCanceled",
                taskId
            };

            SendToWindowsServer(payload);
        }

        public RdpTask GetUpcomingTask()
        {
            var payload = new
            {
                command = "GetUpcomingTask"
            };

            string json = JsonSerializer.Serialize(payload);

            using (_client)
            {
                _client.Connect(3000);

                // שליחה
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                _client.Write(buffer, 0, buffer.Length);
                _client.Flush();

                // קריאה תשובה
                using (var reader = new StreamReader(_client, Encoding.UTF8, false, 1024, true))
                {
                    string responseJson = reader.ReadLine(); // עד סוף שורה
                    if (string.IsNullOrWhiteSpace(responseJson))
                        return null;

                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() }
                    };
                    return JsonSerializer.Deserialize<RdpTask>(responseJson, options);
                }
            }
        }


        private async Task SendToWindowsServer(object payload)
        {
            if (!_isConnected || _writer == null)
                await ConnectAsync();

            lock (_lock) // מונע כתיבה בו זמנית ממספר threads
            {
                string json = JsonSerializer.Serialize(payload);
                _writer.WriteLine(json);
            }
        }
    }
}
