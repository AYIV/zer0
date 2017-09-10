using System;
using System.Linq;
using zer0.core.Contracts;

namespace zer0.core.Messages
{
	public sealed class CommandMessage : MessageBase, IChannelMessage
	{
		public string Command { get; private set; }

		public string[] Args { get; private set; }
        public string Channel { get; set; }

        public CommandMessage(string message)
			: base(message, MessageType.Command)
		{
			var args = message.Split(' ');

			Command = args[0];
			Args = args.Skip(1).ToArray();
		}

		public CommandMessage(string message, Guid id)
			: base(message, MessageType.Command, id)
		{
		}

		public static IMessage New(string message) => new CommandMessage(message);
	}
}
