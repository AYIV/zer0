using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace zer0.loader.torrent
{
	public class QBittorrentApi : IDisposable
	{
		private readonly string _host;
		private readonly HttpClient _http;

		public QBittorrentApi(string host)
		{
			_host = host;

			_http = new HttpClient();
			_http.DefaultRequestHeaders.Referrer = new Uri(_host);
		}

		public Torrent Get(Uri uri) => Get(uri.Btih());

		public Torrent Get(string btih)
		{
			return Get<Torrent[]>($"{_host}/query/torrents")
				.FirstOrDefault(x => x.Hash.ToLower() == btih);
		}

		public IEnumerable<Torrent> Files(Uri uri) => Files(uri.Btih());

		public IEnumerable<Torrent> Files(string btih) => Get<Torrent[]>($"{_host}/query/propertiesFiles/{btih}");

		public void Add(Uri uri, bool startDownloading = false)
		{
			_http.PostAsync(
				$"{_host}/command/download",
				("urls", uri.AbsoluteUri)
				//("paused", (!startDownloading).ToString().ToLower())
			).Wait();

			if (startDownloading) return;

			Task.Delay(5000).Wait();

			Pause(uri);
		}

		public void Pause(Uri uri) => Pause(uri.Btih());

		public void Pause(string btih)
		{
			_http.PostAsync(
				$"{_host}/command/pause",
				("hash", btih)
			).Wait();
		}

		public void Recheck(Uri uri) => Recheck(uri.Btih());

		public void Recheck(string btih)
		{
			_http.PostAsync(
				$"{_host}/command/recheck",
				("hash", btih)
			).Wait();
		}

		public void Dispose() => _http?.Dispose();

		public T Get<T>(string url)
		{
			var response = _http.GetAsync(url).Result;
			var json = response.Content.ReadAsStringAsync().Result;

			return JsonConvert
				.DeserializeObject<T>(json);
		}
	}
}
