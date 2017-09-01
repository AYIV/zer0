using System;
using zer0.core.Contracts;

namespace zer0.core
{
	public sealed class CommandLoader : IDisposable
	{
		private readonly IObjectFactory _loader;

		public CommandLoader(IObjectFactory loader) =>	_loader = loader;

		public IAction Load(string command, string args) => _loader
			.GetInstance<ICommandFactory>(command)
			.Build(args);

		public void Dispose() => _loader?.Dispose();
	}
}
