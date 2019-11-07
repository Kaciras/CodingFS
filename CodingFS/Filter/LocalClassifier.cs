using System;
using System.Collections.Generic;
using System.Text;

namespace CodingFS.Filter
{
	public sealed class LocalClassifier : Classifier
	{
		public RecognizeType Recognize(string file)
		{
			return file == @"D:\Coding\ThirdParty" ? RecognizeType.Dependency : RecognizeType.NotCare;
		}
	}
}
