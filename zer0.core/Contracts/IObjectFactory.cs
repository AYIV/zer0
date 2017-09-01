using System;
using System.Collections.Generic;

namespace zer0.core.Contracts
{
	public interface IObjectFactory : IDisposable
	{
		T GetInstance<T>();
		IEnumerable<T> GetInstances<T>();
		T GetInstance<T>(string name);
	}
}
