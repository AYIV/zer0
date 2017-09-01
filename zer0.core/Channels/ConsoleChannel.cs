using System;
using System.ComponentModel.Composition;
using System.Threading;
using zer0.core.Contracts;
using zer0.core.Messages;

namespace zer0.core
{
	[Export(typeof(IChannel))]
	public class ConsoleChannel : ChannelBase, ISelfManagingChannel
	{
		public override string Provider => "Console";

		private bool _stopProcessing;
		
		public override bool Process(IMessage message)
		{
			Console.WriteLine(message.Message);

			return true;
		}

		public void Start() => ThreadPool.QueueUserWorkItem(e =>
		{
			while (!_stopProcessing) ToZer0(new TextMessage(Console.ReadLine(), Guid.NewGuid()));
		});

		public void Stop() => _stopProcessing = true;

		public override bool Supports(IMessage message)
		{
			throw new NotImplementedException();
		}
	}
}
