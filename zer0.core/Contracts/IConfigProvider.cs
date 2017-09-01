using System.Collections.Generic;

namespace zer0.core.Contracts
{
	public interface IConfigProvider : IModule
	{
		T Key<T>(string key);

		IEnumerable<T> Keys<T>();
	}
}
