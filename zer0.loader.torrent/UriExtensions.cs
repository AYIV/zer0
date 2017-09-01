using System;
using System.Text.RegularExpressions;

namespace zer0.loader.torrent
{
	static class UriExtensions
	{
		private static readonly Regex MagnetBtih = new Regex(@"\b([a-f0-9]{40})\b", RegexOptions.Compiled);

		public static bool IsMagnet(this Uri @uri) => @uri.Scheme == "magnet";

		public static string Btih(this Uri @uri)
		{
			if (!@uri.IsMagnet()) return null;

			return MagnetBtih
				.Match(uri.AbsoluteUri)
				.Value;
		}
	}
}
