using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CodingFS.Filter;

namespace CodingFS
{
	public class CodingFileScanner
	{
		readonly PathTrie<IList<Classifier>> trie = new PathTrie<IList<Classifier>>(null);



		public IEnumerable<FileSystemInfo> Scan(string directory)
		{
			throw new NotImplementedException();
		}
	}
}
