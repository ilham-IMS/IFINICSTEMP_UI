using System.Text;
using System.Text.Json;
using iFinancing360.UI.Helper.APIClient;

namespace Config
{
	public static class HttpClientExtensions
	{
		private static Dictionary<string, string> URLList = new Dictionary<string, string>()
		{
			["IFINBASE"] = "http://147.139.191.88:7000/ifinbase",
			["IFINSYS"] = "http://147.139.191.88:7000/ifinsys",
			["IFINCORE"] = "http://147.139.191.88:7000/ifincore",
			["IFINCMS"] = "http://147.139.191.88:7000/ifincms",
			["IFINSLIK"] = "http://147.139.191.88:7000/ifinslik",
			["IFINSIPP"] = "http://147.139.191.88:7000/ifinsipp",
			["IFINLOS"] = "http://147.139.191.88:7000/ifinlos",
			["IFINDOC"] = "http://147.139.191.88:7000/ifindoc",
			["IFINSVY"] = "http://147.139.191.88:7000/ifinsvy",
			["IFINSCR"] = "http://147.139.191.88:7000/ifinscr",
			["IFINLGL"] = "http://147.139.191.88:7000/ifinlgl",
			["IFININS"] = "http://147.139.191.88:7000/ifinins",
			["IFINPBS"] = "http://147.139.191.88:7000/ifinpbs",
			["IFINPDC"] = "http://147.139.191.88:7000/ifinpdc",
			["IFINAPV"] = "http://147.139.191.88:7000/ifinapv",
      ["IFINICS"] = "http://localhost:5200",
		};

		public static void AddAPIClient(this IServiceCollection services)
		{
			services.AddTransient<HeaderHandler>(_ => new HeaderHandler("bmltZEE="));

			services.AddHttpClient<IFINSYSClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINSYS") ?? URLList["IFINSYS"]));
			services.AddHttpClient<IFINBASEClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINBASE") ?? URLList["IFINBASE"]));
			services.AddHttpClient<IFINCMSClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINCMS") ?? URLList["IFINCMS"]));
			services.AddHttpClient<IFINCOREClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINCORE") ?? URLList["IFINCORE"]));
			services.AddHttpClient<IFINSIPPClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINSIPP") ?? URLList["IFINSIPP"]));
			services.AddHttpClient<IFINSLIKClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINSLIK") ?? URLList["IFINSLIK"]));
			services.AddHttpClient<IFINLOSClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINLOS") ?? URLList["IFINLOS"]));
			services.AddHttpClient<IFINAPVClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINAPV") ?? URLList["IFINAPV"]));
			services.AddHttpClient<IFININSClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFININS") ?? URLList["IFININS"]));
			services.AddHttpClient<IFINLGLClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINLGL") ?? URLList["IFINLGL"]));
			services.AddHttpClient<IFINPDCClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINPDC") ?? URLList["IFINPDC"]));
			services.AddHttpClient<IFINPBSClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINPBS") ?? URLList["IFINPBS"]));
			services.AddHttpClient<IFINSCRClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINSCR") ?? URLList["IFINSCR"]));
			services.AddHttpClient<IFINSVYClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINSVY") ?? URLList["IFINSVY"]));
      services.AddHttpClient<IFINICSClient>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IFINICS") ?? URLList["IFINICS"]));
		}

		public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient httpClient, string requestUri, T data)
			 => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = Serialize(data ?? default!) });

		public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient httpClient, string requestUri, T data, CancellationToken cancellationToken)
			 => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = Serialize(data ?? default!) }, cancellationToken);

		public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient httpClient, Uri requestUri, T data)
			 => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = Serialize(data ?? default!) });

		public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient httpClient, Uri requestUri, T data, CancellationToken cancellationToken)
			 => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = Serialize(data ?? default!) }, cancellationToken);

		private static HttpContent Serialize(object data) => new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
	}

	public class HeaderHandler(string _headerValue) : DelegatingHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			request.Headers.Add("User", _headerValue);
			return await base.SendAsync(request, cancellationToken);
		}

	}
}