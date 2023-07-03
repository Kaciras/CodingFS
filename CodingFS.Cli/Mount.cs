using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodingFS.FUSE;
using CommandLine.Text;
using CommandLine;
using DokanNet;
using DokanNet.Logging;

namespace CodingFS.Cli;

[Verb("mount", HelpText = "挂载为虚拟磁盘，仅包含指定类型的文件")]
internal class Mount : Command
{
	[Option('p', "point", Default = "x", HelpText = "指定盘符")]
	public string Point { get; set; } = "x";

	[Option('t', "type", HelpText = "要包含的文件类型")]
	public FileType Type { get; set; } = FileType.SourceFile;

	public void Execute()
	{
		var map = new Dictionary<string, CodingPathFilter>()
		{
			["Coding"] = new CodingPathFilter(@"D:\Coding")
		};

		using var _ = new VirtualFS(map, new()
		{
#if DEBUG
			Debug = true,
#else
			Debug = false,
#endif
			Readonly = true,
			Type = Type,
			MountPoint = @$"{Point}:\",
		});

		new ManualResetEvent(false).WaitOne();
	}
}
