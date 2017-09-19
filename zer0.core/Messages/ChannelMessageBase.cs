using System;
using zer0.core.Contracts;

namespace zer0.core.Messages
{
    public abstract class ChannelMessageBase : MessageBase, IChannelMessage
    {
        public ChannelMessageBase(object message, MessageType type)
            : base(message, type)
        {
        }

        public ChannelMessageBase(object message, MessageType type, Guid id)
            : base(message, type, id)
        {
        }

        public string Channel { get; set; }
    }
}
