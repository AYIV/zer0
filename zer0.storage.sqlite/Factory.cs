using System.ComponentModel.Composition;
using zer0.core.Contracts;

namespace zer0.storage.sqlite
{
	[Export(typeof(IConfigProviderFactory))]
	public sealed class Factory : IConfigProviderFactory
	{
		public IConfigProvider Build(IModule module) => new SQLiteStorage(module);
	}
}
