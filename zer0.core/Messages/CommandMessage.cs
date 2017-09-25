using System;
using System.Collections.Generic;
using System.Linq;
using zer0.core.Contracts;

namespace zer0.core.Messages
{
    public interface ICommand : IMessage
    {
        string Name { get; }

        IEnumerable<string> Arguments { get; }
    }

	public sealed class Command : ChannelMessageBase, ICommand
	{
		public string Name { get; private set; }

		public IEnumerable<string> Arguments { get; private set; }

        public Command(string message)
			: base(message, null, MessageType.Command)
		{
			var args = message.Split(' ');

			Name = args[0];
            Arguments = args.Skip(1).ToArray();
		}

		public Command(string message, Guid id)
			: base(message, null, MessageType.Command, id)
		{
		}

		public Command(string message, object content, MessageType type)
			: base(message, content, type)
		{
		}

		public static IMessage New(string message) => new Command(message);
	}

	public sealed class Message : MessageBase
	{
		public Message(string message, object content, MessageType type) : base(message, content, type)
		{
		}

		public static IMessage New(string message, byte[] content) => new Message(message, content, MessageType.Audio);
	}
}
