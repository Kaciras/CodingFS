using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingFS.Filter
{
	[Flags]
	public enum RecognizeType
	{
		NotCare = 0,
		Uncertain = 1,
		Ignored = 2,
		Dependency = 4,
	}
}
