using System;

namespace zer0.core.Commands
{
	internal class RingAlarm : ActionBase
	{
		public RingAlarm(DateTime time)
		{
			StartTime = time;
		}

		public override string Log() => $"Ring Ding it's {DateTime.Now.ToShortTimeString()} already!";
	}
}
