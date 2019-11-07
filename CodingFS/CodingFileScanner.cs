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

		// Directory 和 Path 都跟IO库里的镜头类重名了
		public string FullName { get; }

		private readonly PathTrieNode<Classifier[]> root;

		public CodingFileScanner(string directory, Classifier[] globals)
		{
			FullName = directory;
			root = new PathTrieNode<Classifier[]>(globals);
		}

		public IEnumerable<Classifier> GetClassifiers(string file)
		{
			// 未检查是否属于directory下
			var relative = Path.GetRelativePath(FullName, file);
			var parts = relative.Split(Path.DirectorySeparatorChar);

			var result = root.Value;
			var node = root;

			for (int i = 0; i < parts.Length; i++)
			{
				var part = parts[i];

				if (node.TryGetChild(part, out var child))
				{
					node = child;
				}

				var tempDir = Path.Join(FullName, string.Join('\\', parts.Take(i)));

				var matches = factories
					.Select(f => f.Match(tempDir))
					.Where(x => x != null)!
					.ToArray<Classifier>();

				node = node.PutChild(part, matches);

				if (node.Value.Length > 0)
				{
					return result.Concat(node.Value);
				}
			}

			return result;
		}

		public FileType GetFileType(string path)
		{
			return GetFileType(GetClassifiers(path), path);
		}

		public static FileType GetFileType(IEnumerable<Classifier> classifiers, string path)
		{
			var recogined = classifiers
				.Aggregate(RecognizeType.NotCare, (v, c) => v | c.Recognize(path));

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
