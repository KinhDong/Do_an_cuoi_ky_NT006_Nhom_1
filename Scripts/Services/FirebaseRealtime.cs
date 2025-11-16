using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NT106.Scripts.Services
{
    public class FirebaseStream : IDisposable
    {
        private readonly HttpClient _client;
        private CancellationTokenSource _cts;
        private Task _listenTask;

        public FirebaseStream(HttpClient client = null)
        {
            _client = client ?? new HttpClient();
        }


        public void Start(string url, Action<string> onData, Action<Exception> onError = null)
        {
            Stop();
            _cts = new CancellationTokenSource();

            _listenTask = Task.Run(async () =>
            {
                try
                {
                    var req = new HttpRequestMessage(HttpMethod.Get, url);
                    req.Headers.Add("Accept", "text/event-stream");

                    using var resp = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
                    resp.EnsureSuccessStatusCode();

                    using var stream = await resp.Content.ReadAsStreamAsync(_cts.Token);
                    using var reader = new StreamReader(stream, Encoding.UTF8);

                    var sb = new StringBuilder();
                    while (!_cts.Token.IsCancellationRequested && !reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();
                        if (line == null) break;

                        if (line.StartsWith("data:"))
                        {
                            string dataPart = line.Substring(5).Trim();
                            
                            sb.Append(dataPart);
                        }
                        else if (string.IsNullOrWhiteSpace(line))
                        {
                            if (sb.Length > 0)
                            {
                                string payload = sb.ToString();
                                try
                                {
                                    onData?.Invoke(payload);
                                }
                                catch (Exception ex)
                                {
                                    onError?.Invoke(ex);
                                }
                                sb.Clear();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            }, _cts.Token);
        }

        public void Stop()
        {
            try
            {
                if (_cts != null && !_cts.IsCancellationRequested)
                    _cts.Cancel();
            }
            catch { }
            _cts = null;
            _listenTask = null;
        }

        public void Dispose()
        {
            Stop();
            _client?.Dispose();
        }
    }
}