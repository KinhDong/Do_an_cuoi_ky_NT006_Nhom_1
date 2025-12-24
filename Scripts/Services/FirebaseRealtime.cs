using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NT106.Scripts.Services
{
	public sealed class FirebaseStreaming : IDisposable
	{
		private static readonly HttpClient _httpClient = new HttpClient
		{
			Timeout = Timeout.InfiniteTimeSpan
		};

		private readonly string _url;
		private readonly TimeSpan _idleTimeout;

		private CancellationTokenSource _cts;
		private Task _streamTask;
		private Task _watchdogTask;

		private DateTime _lastEventTime;
		private bool _disposed;

		// ========= EVENTS =========
		public event Action OnConnected;
		public event Action<bool> OnDisconnected;          // true = lỗi
		public event Action<string> OnError;
		public event Action<string, JObject> OnData;        // eventType, payload
		public event Action<TimeSpan> OnIdleTimeout;        // idle duration

		// ========= CTOR =========
		public FirebaseStreaming(
			string baseUrl,
			string path,
			string authToken = null,
			TimeSpan? idleTimeout = null)
		{
			if (!baseUrl.EndsWith("/"))
				baseUrl += "/";

			_url = baseUrl + path + ".json";
			if (!string.IsNullOrEmpty(authToken))
				_url += $"?auth={authToken}";

			_idleTimeout = idleTimeout ?? TimeSpan.FromSeconds(10);
		}

		// ========= PUBLIC =========
		public void Start()
		{
			if (_cts != null)
				throw new InvalidOperationException("Service already started");

			_cts = new CancellationTokenSource();
			_lastEventTime = DateTime.UtcNow;

			_streamTask = Task.Run(() => StreamLoopAsync(_cts.Token));
			_watchdogTask = Task.Run(() => WatchdogLoopAsync(_cts.Token));
		}

		public void Stop()
		{
			_cts?.Cancel();
		}

		// ========= STREAM LOOP =========
		private async Task StreamLoopAsync(CancellationToken token)
		{
			int retryDelay = 1000;
			const int maxRetryDelay = 30000;

			while (!token.IsCancellationRequested)
			{
				bool errorDisconnect = false;

				try
				{
					using var request = new HttpRequestMessage(HttpMethod.Get, _url);
					request.Headers.Add("Accept", "text/event-stream");

					using var response = await _httpClient.SendAsync(
						request,
						HttpCompletionOption.ResponseHeadersRead,
						token
					);

					if (!response.IsSuccessStatusCode)
					{
						OnError?.Invoke(
							$"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}");
						errorDisconnect = true;
						continue;
					}

					OnConnected?.Invoke();
					retryDelay = 1000;
					_lastEventTime = DateTime.UtcNow;

					using var stream = await response.Content.ReadAsStreamAsync(token);
					using var reader = new StreamReader(stream, Encoding.UTF8);

					string eventType = null;
					var dataBuilder = new StringBuilder();

					while (!token.IsCancellationRequested && !reader.EndOfStream)
					{
						var line = await reader.ReadLineAsync();

						// end of SSE message
						if (string.IsNullOrEmpty(line))
						{
							if (eventType != null && dataBuilder.Length > 0)
								ProcessEvent(eventType, dataBuilder.ToString());

							eventType = null;
							dataBuilder.Clear();
							continue;
						}

						if (line.StartsWith("event:"))
							eventType = line.Substring(6).Trim();
						else if (line.StartsWith("data:"))
							dataBuilder.AppendLine(line.Substring(5).Trim());
					}
				}
				catch (OperationCanceledException)
				{
					OnDisconnected?.Invoke(false);
					return;
				}
				catch (Exception ex)
				{
					errorDisconnect = true;
					OnError?.Invoke($"Stream error: {ex.Message}");
				}

				OnDisconnected?.Invoke(errorDisconnect);

				if (token.IsCancellationRequested)
					return;

				await Task.Delay(retryDelay, token);
				retryDelay = Math.Min(retryDelay * 2, maxRetryDelay);
			}
		}

		// ========= WATCHDOG =========
		private async Task WatchdogLoopAsync(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				var idle = DateTime.UtcNow - _lastEventTime;

				if (idle >= _idleTimeout)
				{
					OnIdleTimeout?.Invoke(idle);
					_lastEventTime = DateTime.UtcNow; // tránh spam
				}

				await Task.Delay(1000, token);
			}
		}

		// ========= EVENT PROCESS =========
		private void ProcessEvent(string eventType, string rawData)
		{
			if (rawData.Trim() == "null")
				return;

			try
			{
				var json = JObject.Parse(rawData);
				_lastEventTime = DateTime.UtcNow;

				OnData?.Invoke(eventType, json);
			}
			catch (Exception ex)
			{
				OnError?.Invoke($"JSON parse error: {ex.Message}");
			}
		}

		// ========= DISPOSE =========
		public void Dispose()
		{
			if (_disposed) return;
			_disposed = true;

			Stop();
			_cts?.Dispose();
		}
	}


	public class RoomEvent
	{
		public string type { get; set; }   // "join", "leave", ...
		public string user { get; set; }   // userId
		public string time { get; set; }
		public dynamic payload { get; set; } // Dữ liệu thêm vào (Nếu có)
	}    

	public class MessageEvent
	{
		public string path {get; set;}
		public RoomEvent data {get; set;}
	}
}
