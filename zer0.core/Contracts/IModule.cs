using zer0.core.Messages;

namespace zer0.core.Contracts
{
	public interface IModule
	{
		string Provider { get; }

		bool Initialized { get; }

		void Init(IConfigProvider config);
	}

	public interface IModule<T> : IModule where T : IMessage
	{
		bool Supports(T message);

		bool Process(T message);
	}

	public interface IContextable : IModule
	{
		void Init(IConfigProvider config, ZeroCallback callback);
	}

	public interface IChannel : IModule<IMessage>, IContextable { }

	public interface ILoader : IModule<ICommand>, IContextable { }

    public delegate bool ZeroCallback(IMessage message, IModule module);
}
