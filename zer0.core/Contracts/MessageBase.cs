using System;

namespace zer0.core.Contracts
{
	public class MessageBase : IMessage
	{
		public Guid Id { get; private set; }

		public object Message { get; protected set; }

		public MessageType Type { get; private set; }
		
		public IMessage Context { get; private set; }

		public MessageBase(object message, MessageType type)
			: this(message, type, Guid.NewGuid())
		{
		}

		public MessageBase(object message, MessageType type, Guid id)
		{
			Id = id;
			Message = message;
			Type = type;
		}

		public void SetContext(IMessage contextMessage) => Context = contextMessage;
	}
}
