using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodingFS
{
	public class ProjectInspector
	{
		public IEnumerable<(string, FileType)> Iterate(string dir)
		{
			// EnumerateFiles 和 EnumerateDirectories 都是在 EnumerateFileSystemEntries 上过滤的
			var files = Directory.EnumerateFileSystemEntries(dir);
			throw new Exception();
		}

		public void Print()
		{
			foreach (var item in Iterate(null))
			{

			}
		}
	}
}
