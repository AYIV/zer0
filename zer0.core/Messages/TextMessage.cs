using System;
using zer0.core.Contracts;

namespace zer0.core.Messages
{
	public sealed class TextMessage : MessageBase
	{
		public string Channel { get; private set; }

		public TextMessage(string message, string channel = null)
			: base(message, MessageType.Text)
		{
			Channel = channel;
		}

		public TextMessage(string message, Guid id, string channel = null)
			: base(message, MessageType.Text, id)
		{
			Channel = channel;
		}

		public static IMessage New(string message, string channel = null) => new TextMessage(message, channel);
	}
}
