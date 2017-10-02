using System.Collections.Generic;

namespace zer0.core.Extensions
{
	public static class OfTExtensions
	{
		public static IEnumerable<T> AsEnumerable<T>(this T @this) => new[] { @this };
	}
}
