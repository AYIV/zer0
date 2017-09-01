using System;

namespace zer0.core
{
	public abstract class ActionBase : IAction
	{
		public bool IsDone { get; set; }

		public string Name => GetType().Name;

		public DateTime StartTime { get; set; }

		public abstract string Log();
	}
}
