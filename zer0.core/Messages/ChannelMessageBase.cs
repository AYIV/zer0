using System;
using zer0.core.Contracts;

namespace zer0.core.Messages
{
    public abstract class ChannelMessageBase : MessageBase, IChannelMessage
    {
        private string _channel;

        public ChannelMessageBase(object message, MessageType type)
            : base(message, type)
        {
        }

        public ChannelMessageBase(object message, MessageType type, Guid id)
            : base(message, type, id)
        {
        }

        public string Channel
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_channel))
                    return _channel;

                var root = (IMessage) this;
                while (root.HasContext)
                    root = root.Context;

                return _channel = root is IChannelMessage cmsg
                    ? cmsg.Channel
                    : null;
            }

            set => _channel = value;
        }
    }
}
