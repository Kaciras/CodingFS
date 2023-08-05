using System.Reflection.Emit;
using CommandLine;

namespace CodingFS.Cli;

[Verb("mount", HelpText = "Map a directory to a virtual drive, containing only files of the specified type.")]
public sealed class MountCommand : Command
{
	[Value(0, Required = true, HelpText = "The mount point.")]
	public string Point { get; set; } = string.Empty;

	[Option('l', "label", HelpText = "Volume label in Windows")]
	public string? VolumeLabel { get; set; }

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

	protected override void Execute(Config config)
	{
		var scanner = config.CreateScanner();

		var filter = new MappedPathFilter();
		var top = Path.GetFileName(scanner.Root);
		filter.Set(top, new CodingPathFilter(scanner, Type));

		virtualFS = new VirtualFS(filter, new()
		{
#if DEBUG
			Debug = true,
#else
			Debug = false,
#endif
			Name = VolumeLabel,
			Readonly = true,
			MountPoint = Point,
		});

#if !DEBUG
		Console.WriteLine($"Mouted to {Point}");
#endif

		Console.CancelKeyPress += OnCtrlC;
		AppDomain.CurrentDomain.ProcessExit += OnExit;
		blockMainThreadEvent.WaitOne();
	}
}
