using System.Collections.Generic;

namespace zer0.core.Contracts
{
	public abstract class ConfigProviderBase : ModuleBase, IConfigProvider
	{
		protected string ModuleName { get; private set; }

		protected ConfigProviderBase(IModule module) => ModuleName = module.Provider;

		public abstract T Key<T>(string key);

		public abstract IEnumerable<T> Keys<T>();
	}
}
