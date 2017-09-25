﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using zer0.core.Contracts;
using zer0.core.Messages;

namespace zer0.loader.torrent
{
	[Export(typeof(IModule))]
	public class TorrentLoader : ModuleBase, ILoader, IDisposable
	{
		private static class Commands
		{
			public static string Torrents = "/torrents";
			public static string Files = "/files";
			public static string DownloadOnly = "/downloadonly";

			public static IEnumerable<string> ContextFree = new[] { Torrents };
			public static IEnumerable<string> Contextable = new[] { Files, DownloadOnly };
		}

		public override string Provider => "Torrent";

		//TODO: move to settings
		private const string Host = @"http://localhost:8080";

		private IDictionary<string, Func<IMessage, IMessage>> _supportedCommands;

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
			_supportedCommands = new Dictionary<string, Func<IMessage, IMessage>>
			{
				{ Commands.Files, Files },
				{ Commands.Torrents, GetAll },
				{ Commands.DownloadOnly, DownloadOnly }
			};

			Api = new QBittorrentApi(Host);
		}

		private IMessage GetAll(IMessage message)
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

		private IMessage DownloadOnly(IMessage message)
		{
			if (!torrentContext.ContainsKey(message.Context.Id))
				return TextMessage.New($":( Dunno which torrent you want to explore. Please use BTIH or magnet to proceed.", message);

			var btih = torrentContext[message.Context.Id];
			var torrent = _cache.ContainsKey(btih) ? _cache[btih] : (_cache[btih] = Api.Get(btih));
			var cmd = message as ICommand;

			foreach (var file in torrent.Files)
			{
				file.Priority = Regex.IsMatch(file.Name, cmd.Arguments.First())
					? 7
					: 0;
			}

			Api.SetFilePriority(torrent);

			return TextMessage.New($"Priorities was set successfully! Files to downolad:\n{torrent.Files.Where(x => x.Priority == 7).Select(x => x.Name).Join(",")}");
		}

		public override bool Supports(IMessage message)
		{
			var msg = (string)message.Message;

			if (string.IsNullOrWhiteSpace(msg)) return false;

			if (message.HasContext && Commands.Contextable.Any(msg.StartsWith)) return true;

			if (int.TryParse(msg, out int _) && message.HasContext && Commands.Contextable.Any(((string)message.Context.Message).StartsWith))
				return true;

			if (Commands.ContextFree.Any(msg.StartsWith))
				return true;

			return null != msg
				.Split()
				.LastOrDefault(x => Uri.TryCreate(x, UriKind.Absolute, out Uri uri) && uri.IsMagnet());
		}

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
				var torrent = _cache.Values.ElementAt(index);
				torrentContext[message.Id] = torrent.Hash;

				ToZer0(TextMessage.New($"Current torrent is set.\n{torrent.Name}"), this);
				return true;
			}

			var tokens = msg.ToLower().Split();
			var link = new Uri(tokens.Last());

			Api.Add(link, tokens.Contains("start"));

			var added = Api.Get(link);
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

			var files = Api.Files(torrentContext[message.Context.Id]);
			return TextMessage.New($"{string.Join("\n", files.Select(x => x.Name))}");
		}

		public void Dispose()
		{
			Api?.Dispose();
		}
	}

	public static class StringExtensions
	{
		public static string Join<T>(this IEnumerable<T> enumerable, string separator) => string.Join(separator, enumerable);
	}
}
