using System;
using System.Linq;
using zer0.core.Contracts;

namespace zer0.core.Messages
{
	public sealed class CommandMessage : MessageBase
	{
		public string Command { get; private set; }

		public string Args { get; private set; }

		public CommandMessage(string message)
			: base(message, MessageType.Command)
		{
			var args = message.Split(' ');
			if (args.Length < 2) return;

			Command = args[0];
			Args = string.Join(" ", args.Skip(1));
		}

		public CommandMessage(string message, Guid id)
			: base(message, MessageType.Command, id)
		{
		}

		public static IMessage New(string message) => new CommandMessage(message);
	}
}
