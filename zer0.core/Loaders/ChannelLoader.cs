using System;
using System.Collections.Generic;
using zer0.core.Contracts;
using zer0.core.Extensions;

using IMessage = zer0.core.Contracts.IMessage;

namespace zer0.core
{
	public sealed class ChannelLoader : IDisposable
	{
		private readonly IObjectFactory _loader;
		private readonly IConfigProviderFactory _configFactory;

		public ChannelLoader(IObjectFactory loader)
		{
			_loader = loader;

			_configFactory = _loader.GetInstance<IConfigProviderFactory>();
		}

		public IEnumerable<IChannel> Load(Func<IMessage, bool> send)
		{
			var channels = _loader.GetInstances<IChannel>();

			channels.ForEach(x =>
			{
				var config = _configFactory.Build(x);
				config.Init(null);
				x.Init(config, send);
			});

			return channels;
		}

		public void Dispose() => _loader?.Dispose();
	}
}
