using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using zer0.core.Contracts;

namespace zer0.core
{
	public sealed class Loader : IObjectFactory
	{
		private readonly CompositionContainer _composition;

		private readonly IDictionary<string, object> _cache;

		public Loader()
		{
			_composition = new CompositionContainer(new DirectoryCatalog(".", "zer0*.dll"), true);
			_cache = new Dictionary<string, object>();
		}

		public T GetInstance<T>()
		{
			var name = typeof(T).FullName;

			return (T) (_cache.ContainsKey(name)
				? _cache[name]
				: _cache[name] = _composition.GetExportedValue<T>());
		}

		public T GetInstance<T>(string name) => (T) (_cache.ContainsKey(name)
			? _cache[name]
			: _cache[name] = _composition.GetExportedValue<T>(name));

		public IEnumerable<T> GetInstances<T>() => _composition.GetExportedValues<T>();

		public void Dispose() => _composition?.Dispose();
	}
}
