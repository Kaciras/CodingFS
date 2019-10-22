using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using DokanNet;
using System.Reflection;
using CommandLine;

namespace CodingFS
{
	[Verb("mount", HelpText = "挂载为虚拟磁盘，仅包含指定类型的文件")]
	class MountOptions
	{
		[Option('p', "point", HelpText = "指定盘符")]
		public string Point { get; set; } = "x";

		[Option('t', "type", HelpText = "要包含的文件类型")]
		public FileType Type { get; set; } = FileType.Source;
	}

	static class Program
	{
		static void Main(string[] args)
		{
			var fs = DynamicFSProxy.Create(new CodingFS(@"D:\Coding", @"D:\Project"));

#if !DEBUG
			fs.Mount("x:\\");
#else
			fs.Mount("x:\\", DokanOptions.DebugMode | DokanOptions.StderrOutput);
#endif
		}
	}
}
