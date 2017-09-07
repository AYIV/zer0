using zer0.core.Contracts;

namespace zer0.core
{
	public interface ISelfManagingChannel : IChannel
	{
		void Start();
		void Stop();
	}
}
