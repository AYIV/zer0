using System.Collections.Generic;

namespace zer0.core.Extensions
{
	public static class DictionaryExtensions
	{
		public static TValue Key<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)
			=> @this.ContainsKey(key) ? @this[key] : default(TValue);

		public static IEnumerable<TKey> MissedKeys<TKey, TValue>(this IDictionary<TKey, TValue> @this, IEnumerable<TKey> keys)
		{
			foreach (var key in keys)
			{
				if (@this.ContainsKey(key)) continue;

				yield return key;
			}
		}
	}
}
