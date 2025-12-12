using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;
using System.Security.Principal;
using NT106.Scripts.Models;

namespace NT106.Scripts.Services
{
	public static class FirebaseApi
	{
		public static string BaseUrl = "https://nt106-cf479-default-rtdb.firebaseio.com/";
		public static string ApiKey = "AIzaSyD9_ECO_L-ex-4Iy_FkkstF8c6J2qaaW9Q";
		private static readonly HttpClient client = new();
		

		private static string BuildUrl(string path)
		{
			return BaseUrl + path + ".json?auth=" + UserClass.IdToken;
		}

		public static async Task<T?> Get<T>(string path)
		{
			string url = BuildUrl(path);
			var res = await client.GetStringAsync(url);
			return JsonConvert.DeserializeObject<T>(res);
		}

		public static async Task<string> GetRaw(string path)
		{
			string url = BuildUrl(path);
			return await client.GetStringAsync(url);
		}

		public static async Task Patch<T>(string path, T data)
		{
			string url = BuildUrl(path);
			string json = JsonConvert.SerializeObject(data);

			var method = new HttpMethod("PATCH");
			var req = new HttpRequestMessage(method, url)
			{
				Content = new StringContent(json, Encoding.UTF8, "application/json")
			};

			await client.SendAsync(req);
		}

		public static async Task<bool> Put<T>(string path, T data)
		{
			string url = BuildUrl(path);
			string json = JsonConvert.SerializeObject(data);

			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await client.PutAsync(url, content);
			return response.IsSuccessStatusCode;
		}

		public static async Task Post<T>(string path, T data)
		{
			string url = BuildUrl(path);
			string json = JsonConvert.SerializeObject(data);

			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await client.PostAsync(url, content);
		}

		public static async Task<bool> Delete(string path)
		{
			string url = BuildUrl(path);
			var response = await client.DeleteAsync(url);
			return response.IsSuccessStatusCode;
		}
	}
}
