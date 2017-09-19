using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using zer0.core.Contracts;
using zer0.core.Messages;

namespace zer0.loader.torrent
{
	[Export(typeof(IModule))]
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
                { "/torrents", GetAll }
			};

			_torrent = new QBittorrentApi(Host);
		}

        private IList<Torrent> _torrents = new List<Torrent>();

        private IMessage GetAll(IMessage message)
        {
            var torrents = _torrent.GetAll();
            foreach (var torrent in torrents.Where(x => !_torrents.Contains(x)))
                _torrents.Add(torrent);

            var toString = "";
            for (var i = 0; i < _torrents.Count; i++)
                toString += $"{i}. {_torrents[i].Name}\n{_torrents[i].Hash}\n\n";

            return TextMessage.New(toString);
        }

		public override bool Supports(IMessage message)
		{
			var msg = (string) message.Message;

			if (string.IsNullOrWhiteSpace(msg)) return false;

			if (message.HasContext && _supportedCommands.Keys.Any(msg.StartsWith)) return true;

            if (int.TryParse(msg, out int _) && message.HasContext && IsAnyCommand((string)message.Context.Message))
                return true;

			return null != msg
				.Split()
				.LastOrDefault(x => Uri.TryCreate(x, UriKind.Absolute, out Uri uri) && uri.IsMagnet());
		}

        private bool IsAnyCommand(string message) => _supportedCommands.Keys.Any(message.StartsWith);

		public override bool Process(IMessage message)
		{
			if (!Supports(message)) return false;

			var msg = message is ICommand cmsg
                ? cmsg.Name
                : (string)message.Message;
            msg = msg.Trim();

			if (_supportedCommands.ContainsKey(msg))
			{
				return ToZer0(
                    _supportedCommands[msg](message),
                    this
                );
            }

            if (int.TryParse(msg, out int index))
            {
                var torrent = _torrents[index];
                torrentContext[message.Id] = torrent.Hash;

                ToZer0(TextMessage.New($"Current torrent is set.\n{torrent.Name}"), this);
                return true;
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
				return TextMessage.New($":( Dunno which torrent you want to explore. Please use BTIH or magnet to proceed.", message);

			var files = _torrent.Files(torrentContext[message.Context.Id]);
			return TextMessage.New($"{string.Join("\n", files.Select(x => x.Name))}");
		}

		public void Dispose()
		{
			_torrent?.Dispose();
		}
	}
}
