namespace zer0.core.Contracts
{
	public abstract class ChannelBase : ModuleBase, IContextable
	{
		private ZeroCallback ToZer0 { get; set; }
		
		public void Init(IConfigProvider config, ZeroCallback callback)
		{
			ToZer0 = callback;

			base.Init(config);
		}

        protected bool ToZero(IMessage message) => ToZer0(message, this);
	}
}
