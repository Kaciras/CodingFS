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
			public IList<Classifier> classifiers;
			public IDictionary<int, Node> children;
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

		public void ScanClassifiers(string root)
		{
			Directory.EnumerateDirectories(root);
		}
	}
}
