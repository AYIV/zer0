using System;

namespace zer0.core.Contracts
{
	public abstract class ChannelBase : ModuleBase, IContextable
	{
		protected Func<IMessage, bool> ToZer0 { get; private set; }
		
		public void Init(IConfigProvider config, Func<IMessage, bool> callback)
		{
			ToZer0 = callback;

			base.Init(config);
		}
	}
}
