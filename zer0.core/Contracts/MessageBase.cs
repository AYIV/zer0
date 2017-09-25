using System;

namespace zer0.core.Contracts
{
	public class MessageBase : IMessage
	{
		public Guid Id { get; private set; }

		public string Message { get; protected set; }

		public object Content { get; protected set; }

		public MessageType Type { get; private set; }
		
		public IMessage Context { get; private set; }

		public MessageBase(string message, object content, MessageType type)
			: this(message, content, type, Guid.NewGuid())
		{
		}

		public MessageBase(string message, object content, MessageType type, Guid id)
		{
			Id = id;
			Message = message;
			Content = content;
			Type = type;
		}
		
		public IMessage SetContext(IMessage contextMessage)
		{
			Context = contextMessage;

			return this;
		}

		public bool HasContext => Context != null;
	}
}
