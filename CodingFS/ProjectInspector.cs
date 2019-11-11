using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodingFS.Workspaces;

namespace CodingFS
{
	public class ProjectInspector
	{
		private readonly string directory;
		private readonly FileClassifier classifier;

		public ProjectInspector(string directory, FileClassifier classifier)
		{
			this.directory = directory;
			this.classifier = classifier;
		}

		public IEnumerable<(string, FileType)> Iterate() => Iterate(directory);

		private IEnumerable<(string, FileType)> Iterate(string dir)
		{
			// EnumerateFiles 和 EnumerateDirectories 都是在 EnumerateFileSystemEntries 上过滤的
			var files = Directory.EnumerateFileSystemEntries(dir);

			throw new Exception();
			//foreach (var file in files)
			//{
			//	var recogined = classifiers.Aggregate(RecognizeType.NotCare,
			//		(value, classifier) => value | classifier.Recognize(file));

			//	if (recogined.HasFlag(RecognizeType.Dependency))
			//	{
			//		yield return (file, FileType.Dependency);
			//	}
			//	else if (recogined.HasFlag(RecognizeType.Ignored))
			//	{
			//		yield return (file, FileType.Build);
			//	}
			//	else if (recogined.HasFlag(RecognizeType.Uncertain) && Directory.Exists(file))
			//	{
			//		foreach (var x in Iterate(file))
			//		{
			//			yield return x;
			//		}
			//	}
			//	else
			//	{
			//		yield return (file, FileType.Source);
			//	}
			//}
		}

		public FileGroup Group()
		{
			var groups = new FileGroup();
			foreach (var (file, type) in Iterate())
			{
				groups[type].Add(file);
			}
			return groups;
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

	public sealed class FileGroup
	{
		public ICollection<string> Sources { get; } = new List<string>();
		public ICollection<string> Builds { get; } = new List<string>();
		public ICollection<string> Dependencies { get; } = new List<string>();

		public ICollection<string> this[FileType type] => type switch
		{
			FileType.Source => Sources,
			FileType.Dependency => Dependencies,
			FileType.Build => Builds,
			_ => throw new Exception("未处理的FileType"),
		};
	}
}