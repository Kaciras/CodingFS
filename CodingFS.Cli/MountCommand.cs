using CommandLine;

namespace CodingFS.Cli;

[Verb("mount", HelpText = "Map a directory to a virtual drive, containing only files of the specified type.")]
public sealed class MountCommand : Command
{
	[Value(0, HelpText = "Config file to use.")]
	public string? ConfigFile { get; set; }

	[Option('p', "point", HelpText = "The mount point.")]
	public string Point { get; set; } = "x";

	[Option('t', "type", HelpText = "Which type of files should listed in the file system.")]
	public FileType Type { get; set; } = FileType.Source;

	readonly ManualResetEvent blockMainThreadEvent = new(false);

	VirtualFS virtualFS = null!;

	void OnExit(object? sender, EventArgs e)
	{
		virtualFS.Dispose();
	}

	void OnCtrlC(object? sender, ConsoleCancelEventArgs e)
	{
		virtualFS.Dispose();
		e.Cancel = true;
		blockMainThreadEvent.Set();
	}

	public void Execute()
	{
		var config = Command.LoadConfig(ConfigFile);
		var scanner = config.CreateScanner();

		var filter = new MappedPathFilter();
		var top = Path.GetFileName(config.Root);
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
