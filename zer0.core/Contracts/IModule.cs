using System;

namespace zer0.core.Contracts
{
	public interface IModule
	{
		string Provider { get; }

		bool Initialized { get; }

		void Init(IConfigProvider config);

		bool Supports(IMessage message);

		bool Process(IMessage message);
	}

	public interface IContextable : IModule
	{
		void Init(IConfigProvider config, Func<IMessage, bool> callback);
	}

	public interface IChannel : IContextable { }

	public interface ILoader : IContextable { }
}
