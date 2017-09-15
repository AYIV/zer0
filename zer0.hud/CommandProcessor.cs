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
        private readonly IDictionary<string, IModule> _channels;
        
        public MessageProcessor(IEnumerable<IModule> channels)
        {
            _channels = channels.ToDictionary(x => x.Provider, x => x);
        }

        public void Process(IChannelMessage message)
        {
            if (message.Type == MessageType.None)
                throw new ArgumentException(nameof(message));

            foreach (var ch in _channels)
            {
                // do not send message to creator
                if (ch.Key == message.Channel) continue;

                ch.Value.Process(message);
            }
        }
    }

    internal sealed class CommandProcessor
    {
        private readonly IDictionary<string, IModule> _modules;
        private readonly IDictionary<Guid, IAction> _actions;

        public CommandProcessor(IEnumerable<IModule> modules)
        {
            _modules = modules.ToDictionary(x => x.Provider, x => x);

            _actions = new Dictionary<Guid, IAction>();
        }

        public bool Process(ICommand command) => true == _modules
            .Values
            .FirstOrDefault(x => x.Supports(command))
            ?.Process(command);

        public void UtilizeActions()
        {
            if (!_actions.Any()) return;

            _actions
                .Where(x => !x.Value.IsDone && x.Value.StartTime < DateTime.Now)
                .ForEach(x =>
                {
                    _modules.Values.ForEach(c => c.Process(new TextMessage(x.Value.Log(), x.Key)));
                    x.Value.IsDone = true;
                });
        }
    }
}
