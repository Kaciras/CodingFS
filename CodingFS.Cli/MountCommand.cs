using System.Diagnostics;
using CommandLine;

namespace CodingFS.Cli;

[Verb("mount", HelpText = "Map a directory to a virtual drive, containing only files of the specified type.")]
public sealed class MountCommand : Command
{
	[Value(0, Required = true, HelpText = "The mount point (drive letter).")]
	public string Point { get; set; } = string.Empty;

	[Option('l', "label", HelpText = "Volume label on Windows.")]
	public string? VolumeLabel { get; set; }

	[Option('r', "readonly", HelpText = "Mount the volume as read-only.")]
	public bool Readonly { get; set; } = true;

	[Option('t', "type", HelpText = "Which type of files should be included in the file system. " +
		"Avaliable values: Source, Dependency, Generated, use comma to separate flags.")]
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
		var filter = new MappedPathFilter();
		var scanner = config.CreateScanner();
		var top = Path.GetFileName(scanner.Root);

		if ((Type & FileType.Source) == 0)
		{
			Console.WriteLine($"Type does not contain Source, requires pre-scan files...");
			var watch = new Stopwatch();
			watch.Start();
			filter.Set(top, new PrebuiltPathFilter(scanner, Type, config.MaxDepth));
			Console.WriteLine($"Pre-scan completed in {watch.ElapsedMilliseconds}ms.\n");
		}
		else
		{
			filter.Set(top, new CodingPathFilter(scanner, Type));
		}

		virtualFS = new VirtualFS(filter, new()
		{
#if DEBUG
			Debug = true,
#else
			Debug = false,
#endif
			Name = VolumeLabel,
			Readonly = Readonly,
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
