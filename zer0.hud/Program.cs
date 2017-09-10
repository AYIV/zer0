using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using zer0.core;
using zer0.core.Contracts;
using zer0.core.Extensions;
using zer0.core.Messages;
using IMessage = zer0.core.Contracts.IMessage;

namespace zer0.hud
{
	class Program
	{
		static ConcurrentQueue<IMessage> raw = new ConcurrentQueue<IMessage>();

		static void Main(string[] args)
		{
			Console.Title = "zer0";
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			var lastCommand = (IMessage)null;
			ZeroCallback func = (message, module) =>
			{
                if (module is IChannel && message is IChannelMessage cmsg)
                    cmsg.Channel = module.Provider;

                message.SetContext(lastCommand);

                if (message.Type == MessageType.Command) lastCommand = message;

                raw.Enqueue(message);
				return true;
			};

			var factory = new Loader();
			
			var channels = new ChannelLoader(factory).Load(func);

			channels
				.OfType<ISelfManagingChannel>()
				.ForEach(x => x.Start());

			var loader = factory.GetInstance<ILoader>();
			loader.Init(null, func);

			Sniff(factory, channels, loader);

			while (!raw.Any() || raw.TryPeek(out IMessage lastMessage) && lastMessage.Type != MessageType.None && ((string)lastMessage.Message) != "exit")
			{
				Thread.Sleep(1000);
			}

			channels.ForEach(x => x.Process(TextMessage.New("Zer0 is shutting down. Cya :)")));
		}
		 
		static void Sniff(IObjectFactory factory, IEnumerable<IChannel> channels, ILoader loader)
		{
			ThreadPool.QueueUserWorkItem(e =>
			{
				var cp = new CommandProcessor(new CommandLoader(factory), channels);
				var ma = new MessageAnalyzer(new[] { loader });

				while (true)
				{
					Thread.Sleep(1000);

					cp.UtilizeActions();

					if (!raw.Any()) continue;
					if (!raw.TryDequeue(out IMessage result)) continue;

					//TODO: опросить модули на возможность обработки сообщения (не забыть про контекст переписки)
					// если ни один модуль не увидел в сообщении "шаблон\команду", процессить как обычное текстовое сообщение.
					if (ma.Process(result)) continue;

					cp.Process(result);
				}
			});
		}
	}

	public sealed class MessageAnalyzer
	{
		private readonly IEnumerable<ILoader> _loaders;

		public MessageAnalyzer(IEnumerable<ILoader> loaders)
		{
			_loaders = loaders;
		}

		public bool Process(IMessage message) => true == _loaders
			.FirstOrDefault(x => x.Supports(message))
			?.Process(message);
	}
}
