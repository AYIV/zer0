using System;
using System.ComponentModel.Composition;
using System.Linq;
using zer0.core.Contracts;
using zer0.core.Messages;

namespace zer0.loader.torrent
{
	[Export(typeof(ILoader))]
	public class TorrentLoader : ModuleBase, IContextable, IDisposable
	{
		public override string Provider => "Torrent";

		//TODO: move to settings
		private const string Host = @"http://localhost:8080";

		private Func<IMessage, bool> ToZer0;
		private QBittorrentApi _torrent;

		public void Init(IConfigProvider config, Func<IMessage, bool> callback)
		{
			ToZer0 = callback;

			base.Init(config);
		}

		protected override void SafeInit()
		{
			_torrent = new QBittorrentApi(Host);
		}

		public override bool Supports(IMessage message)
		{
			if (message.Type != MessageType.Text) return false;

			return null != ((string)message.Message)
				.Split()
				.LastOrDefault(x => Uri.TryCreate(x, UriKind.Absolute, out Uri uri) && uri.IsMagnet());
		}

		public override bool Process(IMessage message)
		{
			if (!Supports(message)) return false;

			var tokens = ((string)message.Message).ToLower().Split();
			var link = new Uri(tokens.Last());
			
			_torrent.Add(link, !tokens.Contains("start"));
			
			var added = _torrent.Get(link);
			if (added != null)
				ToZer0(TextMessage.New($"Torrent succesfully added!\n{added.Name}"));

			return added != null;
		}

		public void Dispose()
		{
			_torrent?.Dispose();
		}
	}
}
