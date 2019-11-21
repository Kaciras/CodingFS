using System.Collections.Generic;
using System.IO;

namespace CodingFS
{
	public static class ClassifierExtension
	{
		public static IEnumerable<(string, FileType)> Iterate(this RootFileClassifier classifier, string dir)
		{
			// EnumerateFiles 和 EnumerateDirectories 都是在 EnumerateFileSystemEntries 上过滤的
			foreach (var file in Directory.EnumerateFileSystemEntries(dir))
			{
				var type = classifier.GetFileType(file);

				if (type == FileType.Build)
				{
					yield return (file, FileType.Build);
				}
				else if (type == FileType.Dependency)
				{
					yield return (file, FileType.Dependency);
				}
				else if (Directory.Exists(file))
				{
					foreach (var x in Iterate(classifier, file))
					{
						yield return x;
					}
				}
				else
				{
					yield return (file, FileType.Source);
				}
			}
		}

		public static IEnumerable<(string, FileType)> Iterate(this RootFileClassifier classifier)
		{
			return Iterate(classifier, classifier.Root);
		}

		public static FileGroup Group(this RootFileClassifier classifier, string dir)
		{
			var groups = new FileGroup();
			foreach (var (file, type) in Iterate(classifier, dir))
			{
				groups[type].Add(file);
			}
			return groups;
		}

		public static FileGroup Group(this RootFileClassifier classifier) => Group(classifier, classifier.Root);
	}
}
