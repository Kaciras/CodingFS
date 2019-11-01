using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodingFS
{
	public class ProjectInspector
	{
		private readonly string directory;

		public ProjectInspector(string directory)
		{
			this.directory = directory;
		}

		public IEnumerable<(string, FileType)> Iterate() => Iterate(directory);

		private IEnumerable<(string, FileType)> Iterate(string dir)
		{
			// EnumerateFiles 和 EnumerateDirectories 都是在 EnumerateFileSystemEntries 上过滤的
			var files = Directory.EnumerateFileSystemEntries(dir);

			throw new Exception();
		}

		public Dictionary<FileType, IEnumerable<string>> Group()
		{
			return Iterate()
				.GroupBy((item) => item.Item2, (item) => item.Item1)
				.ToDictionary((group) => group.Key, v => v as IEnumerable<string>);
		}

		public void PrintFiles()
		{
			static void PrintGroup(IEnumerable<string> files, ConsoleColor color, string header)
			{
				Console.ForegroundColor = color;
				Console.WriteLine(header);
				foreach (var file in files) Console.WriteLine(file);
				Console.ResetColor();
			}
			var groups = Group();
			PrintGroup(groups[FileType.Source], ConsoleColor.Cyan, "Source files:");
			PrintGroup(groups[FileType.Dependency], ConsoleColor.Blue, "Dependencies:");
			PrintGroup(groups[FileType.Build], ConsoleColor.Red, "Generated files:");
		}
	}
}
