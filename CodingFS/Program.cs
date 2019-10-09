using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokanNet;

namespace CodingFS
{
	class Program
	{
		static void Main(string[] args)
		{
			var fs = new CodingFS();
			fs.Mount("x:\\", DokanOptions.DebugMode | DokanOptions.StderrOutput,0,130);
		}
	}
}
