using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodingFS.Filter
{
	public class GitVCS : ClassifierFactory
	{
		public Classifier? Match(string path)
		{
			if (Directory.Exists(Path.Combine(path, ".git")))
			{
				return new GitClassifier();
			}
			return null;
		}
	}

	public class GitClassifier : Classifier
	{
		public RecognizeType Recognize(string path)
		{
			return Path.GetFileName(path) == ".git"
				? RecognizeType.Dependency : RecognizeType.NotCare;
		}
	}
}
