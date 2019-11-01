using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingFS.Filter
{
	public interface ClassifierFactory
	{
		Classifier? Match(string path);
	}
}
