using RdpScopeCommands.Stores;
using RdpScopeToggler.Stores;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RdpScopeToggler.Services.PipeClientService
{
    public class PipeClientService : IPipeClientService
    {
        private readonly string _pipeName;
        private NamedPipeClientStream _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        private bool _isListening = false;

        public event Action<string> MessageReceived;
        public bool IsConnected => _client?.IsConnected ?? false;

        public PipeClientService(string pipeName = "RdpScopePipe")
        {
            _pipeName = pipeName;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (_client != null && _client.IsConnected) return; // חיבור יחיד

            _client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await _client.ConnectAsync(cancellationToken);

            _reader = new StreamReader(_client, Encoding.UTF8);
            _writer = new StreamWriter(_client, Encoding.UTF8) { AutoFlush = true };

            if (!_isListening)
            {
                _isListening = true;
                _ = Task.Run(() => ListenLoop(cancellationToken), cancellationToken);
            }
        }

        private async Task ListenLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && IsConnected)
                {
                    string line = await _reader.ReadLineAsync();
                    if (line == null) break;

                    MessageReceived?.Invoke(line);
                }
            }
            catch (Exception ex)
            {
                // אפשר לוג פה
                Console.WriteLine($"PipeClientService ListenLoop error: {ex.Message}");
            }
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

        public void SendAddTask(ActionsEnum action, DateTime date)
        {
            var payload = new
            {
                command = "AddTask",
                task = new
                {
                    Id = Guid.NewGuid().ToString(),
                    Date = date.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Action = action.ToString(),
                    State = "InQueue"
                }
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

            using (var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut))
            {
                client.Connect(3000);

                // שליחה
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                client.Write(buffer, 0, buffer.Length);
                client.Flush();

                // קריאה תשובה
                using (var reader = new StreamReader(client, Encoding.UTF8, false, 1024, true))
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


        private void SendToWindowsServer(object payload)
        {
            string json = JsonSerializer.Serialize(payload);

            using (var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out))
            {
                client.Connect(3000); // timeout 3 שניות
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                client.Write(buffer, 0, buffer.Length);
                client.Flush();
            }
        }
    }
}
