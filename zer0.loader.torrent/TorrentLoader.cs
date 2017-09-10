using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using zer0.core.Contracts;
using zer0.core.Messages;

namespace zer0.loader.torrent
{
	[Export(typeof(ILoader))]
	public class TorrentLoader : ModuleBase, ILoader, IDisposable
	{
		public override string Provider => "Torrent";

		//TODO: move to settings
		private const string Host = @"http://localhost:8080";

		private IDictionary<string, Func<IMessage, IMessage>> _supportedCommands;

		private IDictionary<Guid, string> torrentContext = new Dictionary<Guid, string>();

		private ZeroCallback ToZer0;
		private QBittorrentApi _torrent;

		public void Init(IConfigProvider config, ZeroCallback callback)
		{
			ToZer0 = callback;

			base.Init(config);
		}

		protected override void SafeInit()
		{
			_supportedCommands = new Dictionary<string, Func<IMessage, IMessage>>
			{
				{ "/files", Files },
                { "/torrents", m => TextMessage.New($"{string.Join("\n\n", _torrent.GetAll().Select(x => x.Hash + "\n" + x.Name))}") }
			};

			_torrent = new QBittorrentApi(Host);
		}

		public override bool Supports(IMessage message)
		{
			var msg = (string) message.Message;

			if (string.IsNullOrWhiteSpace(msg)) return false;

			if (message.HasContext && _supportedCommands.Keys.Any(msg.StartsWith)) return true;

			return null != msg
				.Split()
				.LastOrDefault(x => Uri.TryCreate(x, UriKind.Absolute, out Uri uri) && uri.IsMagnet());
		}

		public override bool Process(IMessage message)
		{
			if (!Supports(message)) return false;

			var msg = message is CommandMessage cmsg
                ? cmsg.Command
                : (string)message.Message;
            msg = msg.Trim();

			if (_supportedCommands.ContainsKey(msg))
			{
				return ToZer0(
                    _supportedCommands[msg](message),
                    this
                );
            }

			var tokens = msg.ToLower().Split();
			var link = new Uri(tokens.Last());

			_torrent.Add(link, !tokens.Contains("start"));

			var added = _torrent.Get(link);
			if (added != null)
			{
				ToZer0(TextMessage.New($"Torrent succesfully added!\n{added.Name}"), this);
				torrentContext[message.Id] = added.Hash;
			}

			return added != null;
		}

		private IMessage Files(IMessage message)
		{
			if (!torrentContext.ContainsKey(message.Context.Id))
				return TextMessage.New($":( Dunno which torrent you want to explore. Please use BTIH to proceed.", message);

			var files = _torrent.Files(torrentContext[message.Context.Id]);
			return TextMessage.New($"{string.Join("\n", files.Select(x => x.Name))}");
		}

		public void Dispose()
		{
			_torrent?.Dispose();
		}
	}
}
