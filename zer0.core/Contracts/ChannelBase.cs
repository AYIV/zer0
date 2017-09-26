namespace zer0.core.Contracts
{
	public abstract class ChannelBase : ModuleBase, IChannel
	{
		private ZeroCallback ToZer0 { get; set; }
		
		public void Init(IConfigProvider config, ZeroCallback callback)
		{
			ToZer0 = callback;

			base.Init(config);
		}

		public abstract bool Process(IMessage message);

		public abstract bool Supports(IMessage message);

		protected bool ToZero(IMessage message) => ToZer0(message, this);
	}
}
