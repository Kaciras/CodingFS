using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodingFS.Filter
{
	public class RootClassifier : Classifier
	{
		public RecognizeType Recognize(string directory)
		{
			var name = Path.GetFileName(directory);
			switch (name)
			{
				case "__pycache__":
				case ".git":
				case ".gradle":
					return RecognizeType.Ignored;
			}
			return RecognizeType.NotCare;
		}
	}
}
