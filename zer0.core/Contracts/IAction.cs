using System;

namespace zer0.core
{
	public interface IAction
	{
		bool IsDone { get; set; }
		string Name { get; }
		DateTime StartTime { get; set; }
		string Log();
	}
}
