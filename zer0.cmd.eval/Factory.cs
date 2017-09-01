using System.ComponentModel.Composition;

using zer0.core;

namespace zer0.cmd.eval
{
	[Export("eval", typeof(ICommandFactory))]
	internal sealed class Factory : ICommandFactory
	{
		public IAction Build(string args) => new Eval(args);
	}
}
