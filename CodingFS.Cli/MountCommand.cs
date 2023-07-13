using CommandLine;

namespace CodingFS.Cli;

[Verb("mount", HelpText = "挂载为虚拟磁盘，仅包含指定类型的文件")]
sealed class MountCommand : Command
{
	[Option('p', "point", Default = "x", HelpText = "指定盘符")]
	public string Point { get; set; } = "x";

	[Option('t', "type", HelpText = "Which type of files should listed in the file system.")]
	public FileType Type { get; set; } = FileType.Source;

	public void Execute()
	{
		var scanner = new CodingScanner(@"D:\Coding");
		var filter = new MapPathFilter();
		filter.Set(@"Coding", new CodingPathFilter(scanner, Type));

		using var _ = new VirtualFS(filter, new()
		{
#if DEBUG
			Debug = true,
#else
			Debug = false,
#endif
			Readonly = true,
			MountPoint = @$"{Point}:\",
		});

		Console.WriteLine("Mounted, pass any key to unmount.");
		Console.ReadKey(true);
	}
}
