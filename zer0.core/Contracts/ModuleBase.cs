namespace zer0.core.Contracts
{
	public abstract class ModuleBase : IModule
	{
		protected IConfigProvider Config { get; private set; }

		public abstract string Provider { get; }

		public bool Initialized { get; protected set; }

		public void Init(IConfigProvider config)
		{
			if (Initialized) return;

			Config = config;

			try
			{
				SafeInit();

				Initialized = true;
			}
			catch
			{
				//TODO: implement this when logging will be in place.
				Initialized = false;
			}
		}

		protected virtual void SafeInit() {}

		protected virtual void Failed() {}
	}
}
