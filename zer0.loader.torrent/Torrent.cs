using System.Collections.Generic;

namespace zer0.loader.torrent
{
    public class Torrent
	{
		public string Name { get; set; }
		public string Hash { get; set; }
        public IEnumerable<File> Files { get; set; }
	}

    public class File
    {
        public string Name { get; set; }
        public string Size { get; set; }
        public double Progress { get; set; }
        public int Priority { get; set; }
    }
}
