using System;
using System.Collections.Generic;
using System.Linq;

namespace zer0.core.Extensions
{
	public static class EnumerableExtensions
	{
		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			foreach (var item in items) action(item);
		}

		public static string Join<T>(this IEnumerable<T> enumerable, string separator)
			=> string.Join(separator, enumerable);

		public static IEnumerable<T2> Select<T1, T2>(this IEnumerable<T1> enumerable, Func<int, T1, T2> func, int seed = 0)
			=> Enumerable.Select(enumerable, x => func(seed++, x));
	}
}
