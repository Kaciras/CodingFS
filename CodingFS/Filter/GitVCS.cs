﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodingFS.Filter
{
	public class GitVCS : ClassifierFactory
	{
		public Classifier? TryMatch(string path)
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
		public RecognizeType RecognizeDirectory(string path)
		{
			return Path.GetDirectoryName(path) == ".git"
				? RecognizeType.NotCare
				: RecognizeType.Dependency;
		}
	}
}