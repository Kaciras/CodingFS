using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CodingFS.Filter;

namespace CodingFS
{
	public sealed class CodingFileScanner
	{
		private sealed class Node
		{
			public IList<Classifier> classifiers = Array.Empty<Classifier>();

			public IDictionary<string, Node> children = new Dictionary<string, Node>();
		}

		private static ClassifierFactory[] factories =
		{
			new JetBrainsIDE(),
			new NodeJSFilter(),
			new GitVCS(),
			new VisualStudioIDE(),
		};

		private readonly Node root = new Node();

		public CodingFileScanner(IList<string> directories)
		{
			root.classifiers = new Classifier[] { new RootClassifier() };

			foreach (var item in directories)
			{
				ScanClassifiers(item);
			}
		}

		public void ScanClassifiers(string dir)
		{
			var node = new Node();
			root.children[dir] = node;

			foreach (var project in Directory.EnumerateDirectories(dir))
			{
				var pNode = new Node();
				pNode.classifiers = factories.Select(f => f.Match(dir)).Where(x => x != null).ToList()!;
				node.children[project] = pNode;
			}
		}

		public FileType GetFileType(string file)
		{
			var parts = file.Split(Path.DirectorySeparatorChar);
			var node = root;
			var recogined = RecognizeType.NotCare;

			foreach (var part in parts)
			{
				foreach (var cfd in node.classifiers)
				{
					recogined |= cfd.Recognize(file);
				}
				var @continue = node.children.ContainsKey(part);
				if (!@continue)
				{
					break;
				}
				node = node.children[part];
			}

			if (recogined.HasFlag(RecognizeType.Dependency))
			{
				return FileType.Dependency;
			}
			if (recogined.HasFlag(RecognizeType.Ignored))
			{
				return FileType.Build;
			}
			return FileType.Source;
		}
	}
}
