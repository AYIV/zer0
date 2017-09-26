using System;
using System.Collections.Generic;
using zer0.core.Contracts;
using zer0.core.Extensions;

namespace zer0.core
{
	public sealed class ModuleInitializer : IDisposable
	{
		private readonly IObjectFactory _loader;
		private readonly IConfigProviderFactory _configFactory;

		public ModuleInitializer(IObjectFactory loader)
		{
			_loader = loader;

			_configFactory = _loader.GetInstance<IConfigProviderFactory>();
		}

		public IEnumerable<T> Load<T>(IEnumerable<T> modules, ZeroCallback send) where T: IContextable
		{
			modules.ForEach(x =>
			{
				var config = _configFactory.Build(x);
				config.Init(null);
				x.Init(config, send);
			});

			return modules;
		}

		public void Dispose() => _loader?.Dispose();
	}
}
