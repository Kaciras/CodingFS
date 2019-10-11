using System;
using System.Collections.Generic;
using System.Text;

namespace CodingFS.Filter
{
	public interface Classifier
	{
		RecognizeType RecognizeDirectory(string directory) => RecognizeType.NotCare;

		RecognizeType RecognizeFile(string file) => RecognizeType.NotCare;
	}
}
