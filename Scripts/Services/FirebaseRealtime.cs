using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NT106.Scripts.Services
{
	public class FirebaseStreaming
	{
		private readonly string _url;
		private readonly string _authToken;
		private CancellationTokenSource _cts;

		public event Action<JObject> OnData;
		public event Action<string> OnError;
		public event Action OnConnected;
		public event Action OnDisconnected;

		public FirebaseStreaming(string baseUrl, string path, string authToken = null)
		{
			_authToken = authToken;
			
			if (!baseUrl.EndsWith("/"))
				baseUrl += "/";

			_url = baseUrl + path + ".json";

			if (!string.IsNullOrEmpty(authToken))
				_url += $"?auth={authToken}";
		}

		public void Start()
		{
			_cts = new CancellationTokenSource();
			Task.Run(() => StreamLoop(_cts.Token));
		}

		public void Stop()
		{
			try
			{
				_cts?.Cancel();
			}
			catch { }
		}

		private async Task StreamLoop(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					using var client = new HttpClient();
					client.Timeout = Timeout.InfiniteTimeSpan;

					using var request = new HttpRequestMessage(HttpMethod.Get, _url);
					request.Headers.Add("Accept", "text/event-stream");

					OnConnected?.Invoke();

					using var response = await client.SendAsync(
						request,
						HttpCompletionOption.ResponseHeadersRead,
						token
					);

					if (!response.IsSuccessStatusCode)
					{
						OnError?.Invoke($"HTTP error {response.StatusCode}");
						await Task.Delay(1000); // retry
						continue;
					}

					using var stream = await response.Content.ReadAsStreamAsync(token);
					using var reader = new StreamReader(stream, Encoding.UTF8);

					while (!token.IsCancellationRequested && !reader.EndOfStream)
					{
						var line = await reader.ReadLineAsync();
						if (string.IsNullOrWhiteSpace(line)) continue;

						if (!line.StartsWith("data:")) continue;

						string json = line.Substring(5).Trim();
						if (json == "null") continue;

						try
						{
							JObject obj = JObject.Parse(json);
							OnData?.Invoke(obj);
						}
						catch (Exception ex)
						{
							OnError?.Invoke($"JSON parse error: {ex.Message}");
						}
					}
				}
				catch (Exception ex)
				{
					OnError?.Invoke($"Stream error: {ex.Message}");
				}

				OnDisconnected?.Invoke();

				// Firebase thường đóng kết nối sau 30-60 phút → auto reconnect
				await Task.Delay(1000);
			}
		}
	}

	public class RoomEvent
	{
		public string type { get; set; }   // "join", "leave"
		public string user { get; set; }   // userId
		public long time { get; set; }
		public dynamic payload { get; set; } // Dữ liệu thêm vào (Nếu có)
	}    

	public class MessageEvent
	{
		public string path {get; set;}
		public RoomEvent data {get; set;}
	}
}
