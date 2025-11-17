// FirebaseStream.cs
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Godot;

public partial class FirebaseStream : Node
{
    private System.Net.Http.HttpClient _client = new ();
    private CancellationTokenSource _cts;

    public async void StartListen(string url, Action<string> onData, Action<Exception> onError = null)
    {
        StopListen();
        _cts = new CancellationTokenSource();

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "text/event-stream");

            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var reader = new System.IO.StreamReader(stream);

            string line;
            var dataBuilder = new System.Text.StringBuilder();
            
            while (!_cts.Token.IsCancellationRequested && (line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("data:"))
                {
                    dataBuilder.Append(line.Substring(5).Trim());
                }
                else if (string.IsNullOrEmpty(line) && dataBuilder.Length > 0)
                {
                    onData?.Invoke(dataBuilder.ToString());
                    dataBuilder.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
        }
    }

    public void StopListen()
    {
        _cts?.Cancel();
    }
}