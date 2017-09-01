namespace zer0.core.Contracts
{
	public interface IConfigProviderFactory
	{
		IConfigProvider Build(IModule module);
	}
}
