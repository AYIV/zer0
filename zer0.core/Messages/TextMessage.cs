using System;
using zer0.core.Contracts;

namespace zer0.core.Messages
{
	public sealed class TextMessage : ChannelMessageBase
	{
		public TextMessage(string message)
			: base(message, null, MessageType.Text)
		{
		}

		public TextMessage(string message, Guid id)
			: base(message, null, MessageType.Text, id)
		{
		}

		public static IMessage New(string text) => new TextMessage(text);

		public static IMessage New(string text, IMessage message) => new TextMessage(text).SetContext(message);
	}
}
