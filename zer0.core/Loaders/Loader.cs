using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using zer0.core.Contracts;

namespace zer0.core
{
	public sealed class Loader : IObjectFactory
	{
		private readonly CompositionContainer _composition;

        private readonly IDictionary<string, object> _cache;

        private IEnumerable<IModule> _modules;

		public IEnumerable<IModule> Modules
        {
            get => _modules ?? (_modules = _composition.GetExportedValues<IModule>());
        }

		public Loader()
		{
			_composition = new CompositionContainer(new DirectoryCatalog(".", "zer0*.dll"), true);
			_cache = new Dictionary<string, object>();
		}

		public T GetInstance<T>() where T : class
		{
			var name = typeof(T).FullName;
            
			return (T) (_cache.ContainsKey(name)
				? _cache[name]
				: _cache[name] = Modules.OfType<T>().FirstOrDefault() ?? _composition.GetExportedValue<T>());
		}

		public T GetInstance<T>(string name) => (T) (_cache.ContainsKey(name)
			? _cache[name]
			: _cache[name] = _composition.GetExportedValue<T>(name));

		public IEnumerable<T> GetInstances<T>() => Modules.OfType<T>();

		public void Dispose() => _composition?.Dispose();
	}
}
