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

        public IEnumerable<Torrent> GetAll() => Get<Torrent[]>($"{_host}/query/torrents");

        public Torrent Get(Uri uri) => Get(uri.Btih());

        public Torrent Get(string btih)
        {
            var torrent = GetAll().FirstOrDefault(x => x.Hash.ToLower() == btih);
            if (torrent == null) return null;

            torrent.Files = Files(btih);
            return torrent;
        }

        public IEnumerable<File> Files(Uri uri) => Files(uri.Btih());

        public IEnumerable<File> Files(string btih) => Get<File[]>($"{_host}/query/propertiesFiles/{btih}");

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

        public void Pause(string btih) =>
            _http.PostAsync($"{_host}/command/pause", ("hash", btih)).Wait();

        public void Recheck(Uri uri) => Recheck(uri.Btih());

        public void Recheck(string btih) =>
            _http.PostAsync($"{_host}/command/recheck", ("hash", btih)).Wait();

        public void Dispose() => _http?.Dispose();

        public T Get<T>(string url)
        {
            var response = _http.GetAsync(url).Result;
            var json = response.Content.ReadAsStringAsync().Result;

            return JsonConvert
                .DeserializeObject<T>(json);
        }

        public void SetFilePriority(Torrent torrent)
        {
            var files = torrent.Files.ToArray();

            for (var i = 0; i < files.Length; i++)
                _http.PostAsync($"{_host}/command/setFilePrio",
					("hash", torrent.Hash),
					("id", i),
					("priority", files[i].Priority)
				).Wait();

            torrent.Files = Files(torrent.Hash);

            var updated = torrent.Files.ToArray();
            for (var i = 0; i < updated.Length; i++)
                if (files[i].Priority != updated[i].Priority) throw new Exception("Priority doesn't changed");
        }
    }
}
