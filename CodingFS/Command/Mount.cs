using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodingFS.VFS;
using CommandLine.Text;
using CommandLine;
using DokanNet;
using DokanNet.Logging;

namespace CodingFS.Command;

[Verb("mount", HelpText = "挂载为虚拟磁盘，仅包含指定类型的文件")]
internal class Mount : CliCommand
{
	[Option('p', "point", Default = "x", HelpText = "指定盘符")]
	public string Point { get; set; } = "x";

	[Option('t', "type", HelpText = "要包含的文件类型")]
	public FileType Type { get; set; } = FileType.SourceFile;

	public void Execute()
	{
		var vfs = new UnsafeCodingFS("CodingFS") { Type = Type };
		vfs.Map["Coding"] = new RootFileClassifier(@"D:\Coding");

#if DEBUG
		using var dokanLogger = new ConsoleLogger("[Dokan] ");
		var mountOptions = DokanOptions.DebugMode | DokanOptions.StderrOutput;
#else
		var mountOptions = default(DokanOptions);
		var dokanLogger = new NullLogger();
		Console.WriteLine($@"CodingFS mounted at x:\");
#endif

		using var dokan = new Dokan(dokanLogger);
		using var instance = new DokanInstanceBuilder(dokan)
			.ConfigureOptions(options =>
			{
				options.MountPoint = $"{Point}:\\";
				options.Options = mountOptions;
			})
			.Build(new StaticFSWrapper(vfs));

		new ManualResetEvent(false).WaitOne();
	}
}
