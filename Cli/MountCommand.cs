using System.Diagnostics;
using CommandLine;

namespace CodingFS.Cli;

[Verb("mount", HelpText = "Map a directory to a virtual drive, containing only files of the specified type.")]
public sealed class MountCommand : Command
{
	[Value(0, HelpText = "The mount point (drive letter).")]
	public string? Point { get; set; }

	[Option('l', "label", HelpText = "Volume label on Windows.")]
	public string? VolumeLabel { get; set; }

	[Option('r', "readonly", HelpText = "Mount the volume as read-only.")]
	public bool? Readonly { get; set; }

	[Option('t', "type", HelpText = "Which type of files should be included in the file system. " +
		"Avaliable values: Source, Dependency, Generated, use comma to separate flags.")]
	public FileType? Type { get; set; }

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
		// Apply default options, the command line takes precedence.
		var mountOptions = config.Mount;
		Point ??= mountOptions.Point;
		VolumeLabel ??= mountOptions.VolumeLabel;
		Type ??= mountOptions.Type;
		Readonly ??= mountOptions.Readonly;

		config.Mount.VolumeLabel = "";

		// Create the vitrual directory.
		CreatePathFilter(config, out var filter);

		virtualFS = new VirtualFS(filter, new()
		{
#if DEBUG
			Debug = true,
#else
			Debug = false,
#endif
			MountPoint = Point,
			Name = VolumeLabel,
			Readonly = Readonly.Value,
		});

#if !DEBUG
		Console.WriteLine($"Mouted to {Point}");
#endif

		Console.CancelKeyPress += OnCtrlC;
		AppDomain.CurrentDomain.ProcessExit += OnExit;
		blockMainThreadEvent.WaitOne();
	}

	void CreatePathFilter(Config config, out MappedPathFilter filter)
	{
		var scanner = config.CreateScanner();
		var top = Path.GetFileName(scanner.Root);

		filter = new MappedPathFilter();

		if ((Type & FileType.Source) == 0)
		{
			Console.Write($"Type does not contain Source, requires pre-scan files...");
			var watch = new Stopwatch();
			watch.Start();
			filter.Set(top, new PrebuiltPathFilter(scanner, Type!.Value));

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"Completed in {watch.ElapsedMilliseconds}ms.\n");
			Console.ForegroundColor = ConsoleColor.Gray;
		}
		else
		{
			filter.Set(top, new CodingPathFilter(scanner, Type!.Value));
		}
	}
}
