using System;
using zer0.core.Contracts;

namespace zer0.core.Messages
{
	public sealed class TextMessage : MessageBase, IChannelMessage
	{
		public string Channel { get; set; }

		public TextMessage(string message)
			: base(message, MessageType.Text)
		{
		}

		public TextMessage(string message, Guid id)
			: base(message, MessageType.Text, id)
		{
		}

		public static IMessage New(string text) => new TextMessage(text);

		public static IMessage New(string text, IMessage message) => new TextMessage(text).SetContext(message);
	}
}
