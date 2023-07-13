using CommandLine;

namespace CodingFS.Cli;

[Verb("mount", HelpText = "Map a directory to a virtual drive, containing only files of the specified type.")]
sealed class MountCommand : Command
{
	[Value(0, HelpText = "Directory to map, default is current directory.")]
	public string Root { get; set; } = Environment.CurrentDirectory;

	[Option('p', "point", HelpText = "The mount point.")]
	public string Point { get; set; } = @"x:\";

	[Option('t', "type", HelpText = "Which type of files should listed in the file system.")]
	public FileType Type { get; set; } = FileType.Source;

	public void Execute()
	{
		var filter = new MapPathFilter();
		filter.Set(Path.GetFileName(Root), new CodingPathFilter(Root, Type));

		using var _ = new VirtualFS(filter, new()
		{
#if DEBUG
			Debug = true,
#else
			Debug = false,
#endif
			Readonly = true,
			MountPoint = Point,
		});

		Console.WriteLine("Mounted, pass any key to unmount.");
		Console.ReadKey(true);
	}
}
