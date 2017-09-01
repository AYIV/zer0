using System;
using System.Collections.Generic;
using System.Linq;

using zer0.core;
using zer0.core.Contracts;
using zer0.core.Extensions;
using zer0.core.Messages;

namespace zer0.hud
{
	internal sealed class CommandProcessor
	{
		private readonly CommandLoader _loader;
		private readonly IDictionary<string, IChannel> _channels;

		private readonly IDictionary<Guid, IAction> _actions;
		
		public CommandProcessor(CommandLoader loader, IEnumerable<IChannel> channels)
		{
			_loader = loader;
			_channels = channels.ToDictionary(x => x.Provider, x => x);
			_actions = new Dictionary<Guid, IAction>();
		}

		public void Process(IMessage message)
		{
			if (message.Type == MessageType.None)
				throw new ArgumentException(nameof(message));

			if (message.Type == MessageType.Command)
			{
				var cmd = (CommandMessage) message;

				_actions.Add(message.Id, _loader.Load(cmd.Command, cmd.Args));
			}

			if (message.Type == MessageType.Text)
			{
				var channel = ((TextMessage)message).Channel ?? string.Empty;
				if (_channels.ContainsKey(channel))
					_channels[channel].Process(message);
				else
					_channels.Values.ForEach(x => x.Process(message));
			}
		}

		public void UtilizeActions()
		{
			if (!_actions.Any()) return;

			_actions
				.Where(x => !x.Value.IsDone && x.Value.StartTime < DateTime.Now)
				.ForEach(x =>
				{
					_channels.Values.ForEach(c => c.Process(new TextMessage(x.Value.Log(), x.Key)));
					x.Value.IsDone = true;
				});
		}
	}
}
