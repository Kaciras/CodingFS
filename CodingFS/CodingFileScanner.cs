using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CodingFS.Filter;

namespace CodingFS
{
	public sealed class CodingFileScanner
	{
		private static readonly ClassifierFactory[] factories =
		{
			new JetBrainsIDE(),
			new NodeJSFilter(),
			new VisualStudioIDE(),
		};

		private static readonly RootClassifier rootClassifier = new RootClassifier();

		// Directory 和 Path 都跟IO库里的镜头类重名了
		public string FullName { get; }

		private readonly PathTrieNode<Classifier[]> root;

		public CodingFileScanner(string directory)
		{
			FullName = directory;
			root = new PathTrieNode<Classifier[]>(Array.Empty<Classifier>());
		}

		public Classifier[] GetClassifiers(string file)
		{
			// 未检查是否属于directory下
			var relative = Path.GetRelativePath(FullName, file);
			var parts = relative.Split(Path.DirectorySeparatorChar);

			var node = root;

			for (int i = 0; i < parts.Length; i++)
			{
				var part = parts[i];

				if (!node.TryGetChild(part, out var child))
				{
					var tempDir = FullName + Path.DirectorySeparatorChar + 
						string.Join(Path.DirectorySeparatorChar, parts.Take(i));

					var matches = factories
						.Select(f => f.Match(tempDir))
						.Where(x => x != null)!
						.ToArray<Classifier>();

					child = new PathTrieNode<Classifier[]>(matches);
					node.PutChild(part, child);
				}

				if (child!.Value.Length > 0)
				{
					return child.Value;
				}

				node = child;
			}

			return Array.Empty<Classifier>();
		}

		public FileType GetFileType(string path)
		{
			return GetFileType(GetClassifiers(path), path);
		}

		public static FileType GetFileType(Classifier[] classifiers, string path)
		{
			var recogined = classifiers.Aggregate(rootClassifier.Recognize(path),
					(value, classifier) => value | classifier.Recognize(path));

			if (recogined.HasFlag(RecognizeType.Dependency))
			{
				return FileType.Dependency;
			}
			else if (recogined.HasFlag(RecognizeType.Ignored))
			{
				return FileType.Build;
			}
			else
			{
				return FileType.Source;
			}
		}
	}
}
