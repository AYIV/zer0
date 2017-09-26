using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using zer0.core.Contracts;
using zer0.core.Extensions;
using zer0.core.Messages;

namespace zer0.loader.torrent
{
	[Export(typeof(IModule))]
	public class TorrentLoader : ModuleBase, ILoader, IDisposable
	{
		private static class Commands
		{
			public static string Torrents = ".torrents";
			public static string Files = ".files";
			public static string DownloadOnly = ".downloadonly";

			public static IEnumerable<string> ContextFree = new[] { Torrents };
			public static IEnumerable<string> Contextable = new[] { Files, DownloadOnly };
		}

		public override string Provider => "Torrent";

		//TODO: move to settings
		private const string Host = @"http://localhost:8080";
		private const string RootFolder = @"D:\Shared\Torrents";

		private IDictionary<string, Func<ICommand, IMessage>> _supportedCommands;

		private IDictionary<Guid, string> torrentContext = new Dictionary<Guid, string>();
		private IDictionary<string, Torrent> _cache = new Dictionary<string, Torrent>();

		private ZeroCallback ToZer0;
		private QBittorrentApi Api { get; set; }

		public void Init(IConfigProvider config, ZeroCallback callback)
		{
			ToZer0 = callback;

			base.Init(config);
		}

		protected override void SafeInit()
		{
			_supportedCommands = new Dictionary<string, Func<ICommand, IMessage>>
			{
				{ Commands.Files, Files },
				{ Commands.Torrents, GetAll },
				{ Commands.DownloadOnly, DownloadOnly }
			};

			Api = new QBittorrentApi(Host);
		}

		private IMessage GetAll(ICommand command)
		{
			var torrents = Api.GetAll();
			foreach (var torrent in torrents.Except(_cache.Values))
			{
				torrent.Files = Api.Files(torrent.Hash);
				_cache.Add(torrent.Hash, torrent);
			}
			
			var i = 0;

			return TextMessage.New(
				_cache.Values.Select(x => $"{i++}. {x.Name}\n{x.Hash}").Join("\n\n")
			);
		}

		private IMessage DownloadOnly(ICommand command)
		{
			if (!torrentContext.ContainsKey(command.Context.Id))
				return TextMessage.New($":( Dunno which torrent you want to explore. Please use BTIH or magnet to proceed.", command);

			var btih = torrentContext[command.Context.Id];
			var torrent = _cache.ContainsKey(btih) ? _cache[btih] : (_cache[btih] = Api.Get(btih));

			foreach (var file in torrent.Files)
			{
				file.Priority = Regex.IsMatch(file.Name, command.Arguments.First())
					? 7
					: 0;
			}

			Api.SetFilePriority(torrent);

			return TextMessage.New($"Priorities was set successfully! Files to downolad:\n{torrent.Files.Where(x => x.Priority == 7).Select(x => x.Name).Join(",")}");
		}

		public bool Supports(ICommand message)
		{
			if (string.IsNullOrWhiteSpace(message.Message)) return false;

			if (message.HasContext && Commands.Contextable.Any(message.Message.StartsWith)) return true;

			if (int.TryParse(message.Message, out int _)
				&& message.HasContext
				&& Commands.Contextable.Any(message.Context.Message.StartsWith))
				return true;

			if (Commands.ContextFree.Any(message.Message.StartsWith))
				return true;

			return null != message.Message
				.Split()
				.LastOrDefault(x => Uri.TryCreate(x, UriKind.Absolute, out Uri uri) && uri.IsMagnet());
		}

		public bool Process(ICommand command)
		{
			if (!Supports(command)) return false;
			
			if (_supportedCommands.ContainsKey(command.Name))
			{
				return ToZer0(
					_supportedCommands[command.Name](command),
					this
				);
			}

			if (int.TryParse(command.Name, out int index))
			{
				var torrent = _cache.Values.ElementAt(index);
				torrentContext[command.Id] = torrent.Hash;

				ToZer0(TextMessage.New($"Current torrent is set.\n{torrent.Name}"), this);
				return true;
			}
			
			var link = new Uri(command.Name);

			Api.Add(link, command.Arguments.Contains("start"));

			var added = Api.Get(link);
			if (added != null)
			{
				ToZer0(TextMessage.New($"Torrent succesfully added!\n{added.Name}"), this);
				torrentContext[command.Id] = added.Hash;
			}

			return added != null;
		}
		
		private IMessage Files(ICommand command)
		{
			if (command == null)
				return TextMessage.New("Could not process meassage. Should be command instead.");

			if (!torrentContext.ContainsKey(command.Context.Id))
				return TextMessage.New($":( Dunno which torrent you want to explore. Please use BTIH or magnet to proceed.", command);

			var btih = torrentContext[command.Context.Id];
			var torrent = _cache.ContainsKey(btih) ? _cache[btih] : (_cache[btih] = Api.Get(btih));

			if (command.Arguments.Any(x => x == "get"))
			{
				torrent.Files.Where(x => x.Progress == 1).ForEach(x =>
				{
					var cmd = Message.New(
						x.Name,
						System.IO.File.ReadAllBytes($@"{RootFolder}\{x.Name}")
					);
					ToZer0(cmd, this);
				});
				
				return TextMessage.New("Files sent.");
			}
			
			return TextMessage.New($"{Api.Files(btih).Select(x => x.Name).Join("\n")}");
		}

		public void Dispose() => Api?.Dispose();
	}
}
