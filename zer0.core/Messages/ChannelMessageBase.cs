using System;
using zer0.core.Contracts;

namespace zer0.core.Messages
{
    public abstract class ChannelMessageBase : MessageBase, IChannelMessage
    {
        public ChannelMessageBase(string message, object content, MessageType type)
            : base(message, content, type)
        {
        }

        public ChannelMessageBase(string message, object content, MessageType type, Guid id)
            : base(message, content, type, id)
        {
        }

        public string Channel { get; set; }
    }
}
