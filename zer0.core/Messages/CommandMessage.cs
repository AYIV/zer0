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
			: base(message, MessageType.Command)
		{
			var args = message.Split(' ');

			Name = args[0];
            Arguments = args.Skip(1).ToArray();
		}

		public Command(string message, Guid id)
			: base(message, MessageType.Command, id)
		{
		}

		public static IMessage New(string message) => new Command(message);
	}
}
