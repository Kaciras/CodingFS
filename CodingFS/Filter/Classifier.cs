using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodingFS.Filter
{
	public interface Classifier
	{
		RecognizeType Recognize(string file);
	}
}
