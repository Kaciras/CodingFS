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
			var name = Path.GetDirectoryName(directory);
			switch (name)
			{
				case "__pycache__":
				case ".gradle":
					return RecognizeType.Ignored;
			}
			return RecognizeType.NotCare;
		}
	}
}
