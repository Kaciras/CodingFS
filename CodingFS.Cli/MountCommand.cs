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

	readonly ManualResetEvent blockMainThreadEvent = new(false);
	
	IDisposable virtualFS = null!;

	void OnExit(object? _, EventArgs e)
	{
		virtualFS.Dispose();
	}

	void OnCtrlC(object? _, ConsoleCancelEventArgs e)
	{
		virtualFS.Dispose();
		e.Cancel = true;
		blockMainThreadEvent.Set();
	}

	public void Execute()
	{
		var scanner = new CodingScanner(Root);

		var filter = new MappedPathFilter();
		var top = Path.GetFileName(Root);
		filter.Set(top, new CodingPathFilter(scanner, Type));

		virtualFS = new VirtualFS(filter, new()
		{
#if DEBUG
			Debug = true,
#else
			Debug = false,
#endif
			Readonly = true,
			MountPoint = Point,
		});

		Console.CancelKeyPress += OnCtrlC;
		AppDomain.CurrentDomain.ProcessExit += OnExit;
		blockMainThreadEvent.WaitOne();
	}
}
