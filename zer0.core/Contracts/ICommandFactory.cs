namespace zer0.core
{
	public interface ICommandFactory
	{
		IAction Build(string args);
	}
}
