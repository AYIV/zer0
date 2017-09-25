using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace zer0.loader.torrent
{
	public static class HttpClientExtensions
	{
		public static Task<HttpResponseMessage> PostAsync(this HttpClient client, string url, params (string key, object value)[] keys) => client
			.PostAsync(
				url,
				new FormUrlEncodedContent(
					keys.Select(x => new KeyValuePair<string, string>(x.key.ToString(), x.value.ToString()))
				)
			);
	}
}
