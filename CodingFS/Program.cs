using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using DokanNet;
using System.Reflection;

namespace CodingFS
{
	class Program
	{
		static void Main(string[] args)
		{
			var codingFS = new CodingFS(@"D:\Coding", @"D:\Project");
			object proxy = DispatchProxy.Create<IDokanOperations, DynamicFSProxy>();
			((DynamicFSProxy)proxy).Wrapped = codingFS;

			var fs = (IDokanOperations)proxy;
			if (args.Contains(" - prod"))
			{
				fs.Mount("x:\\");
			}
			else
			{
				fs.Mount("x:\\", DokanOptions.DebugMode | DokanOptions.StderrOutput);
			}
		}
	}
}
