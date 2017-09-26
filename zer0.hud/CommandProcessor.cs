using System;
using System.Collections.Generic;
using System.Linq;

using zer0.core;
using zer0.core.Contracts;
using zer0.core.Extensions;
using zer0.core.Messages;

namespace zer0.hud
{
    internal sealed class MessageProcessor
    {
        private readonly IDictionary<string, IModule<IMessage>> _channels;
        
        public MessageProcessor(IEnumerable<IModule<IMessage>> channels)
        {
            _channels = channels.ToDictionary(x => x.Provider, x => x);
        }

        public void Process(IMessage message)
        {
            if (message.Type == MessageType.None)
                throw new ArgumentException(nameof(message));

			if (message is IChannelMessage channelMessage)
			{
				foreach (var ch in _channels)
				{
					// do not send message to creator
					if (ch.Key == channelMessage.Channel) continue;

					ch.Value.Process(channelMessage);
				}

				return;
			}

			foreach (var channel in _channels.Values.Where(x => x.Supports(message)))
				channel.Process(message);
        }
    }

    internal sealed class CommandProcessor
    {
        private readonly IDictionary<string, IModule<ICommand>> _modules;
        private readonly IDictionary<Guid, IAction> _actions;

        public CommandProcessor(IEnumerable<IModule<ICommand>> modules)
        {
            _modules = modules.ToDictionary(x => x.Provider, x => x);

            _actions = new Dictionary<Guid, IAction>();
        }

		public bool Process(ICommand command)
		{
			var module = _modules
			  .Values
			  .FirstOrDefault(x => x.Supports(command));
			if (module == null) return false;

			try
			{
				return module.Process(command);
			}
			catch (Exception ex)
			{
				Queue.Message(TextMessage.New($"{module.Provider} failed command processing.\nError is: {ex.Message}"));

				/* TODO::
				 * 1. set as failed module (or inactive)
				 * 2. remove module from modules
				 * 3. take snapshot (memento)
				 * 4. reinit module
				 * 5. add to active again
				 */

				return false;
			}
		}

        public void UtilizeActions()
        {
            if (!_actions.Any()) return;

            _actions
                .Where(x => !x.Value.IsDone && x.Value.StartTime < DateTime.Now)
                .ForEach(x =>
                {
                    _modules.Values.ForEach(c => c.Process(new Command(x.Value.Log(), x.Key)));
                    x.Value.IsDone = true;
                });
        }
    }
}
