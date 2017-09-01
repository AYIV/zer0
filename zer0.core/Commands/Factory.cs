using System;
using System.ComponentModel.Composition;

namespace zer0.core.Commands
{
	[Export("setalarm", typeof(ICommandFactory))]
	internal sealed class Factory : ICommandFactory
	{
		public IAction Build(string args) => new RingAlarm(DateTime.Parse(args));
	}
}
