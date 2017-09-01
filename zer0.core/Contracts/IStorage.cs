using System.Collections.Generic;

namespace zer0.core.Contracts
{
	public interface IStorage : IModule
	{
		T One<T>(string query);

		IEnumerable<T> Many<T>(string query);
	}
}
