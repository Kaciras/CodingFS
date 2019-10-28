using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace CodingFS.Filter
{
	public class NodeJSFilter : ClassifierFactory
	{
		public Classifier? TryMatch(string path)
		{
			if(File.Exists(Path.Combine(path, "package.json")))
			{
				return new NodeJSClassifier(path);
			}
			return null;
		}
	}

	public class NodeJSClassifier : Classifier
	{
		readonly string root;

		public NodeJSClassifier(string root)
		{
			this.root = root;
		}

		public RecognizeType Recognize(string path)
		{
			if (Path.GetFileName(path) == "node_modules")
			{
				return RecognizeType.Dependency;
			}
			if (Path.GetRelativePath(root, path) == "dist")
			{
				return RecognizeType.Ignored;
			}
			return RecognizeType.NotCare;
		}
	}
}
