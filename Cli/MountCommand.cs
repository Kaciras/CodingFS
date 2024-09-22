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
		// Command line arguments takes precedence.
		var mountOptions = config.Mount;
		if (Point != null)
		{
			mountOptions.Point = Point;
		}
		if (VolumeLabel != null)
		{
			mountOptions.VolumeLabel = VolumeLabel;
		}
		if (Type.HasValue)
		{
			mountOptions.Type = Type.Value;
		}
		if (Readonly.HasValue)
		{
			mountOptions.Readonly = Readonly.Value;
		}

		virtualFS = CreateVirtualFS(config);

#if !DEBUG
		Console.WriteLine($"Mouted to {Point}");
#endif

		Console.CancelKeyPress += OnCtrlC;
		AppDomain.CurrentDomain.ProcessExit += OnExit;
		blockMainThreadEvent.WaitOne();
	}

	public static VirtualFS CreateVirtualFS(Config config)
	{
		var scanner = config.CreateScanner();
		var top = Path.GetFileName(scanner.Root);
		var type = config.Mount.Type;
		var filter = new MappedPathFilter();

		if ((type & FileType.Source) == 0)
		{
			Console.Write("Pre-scan files...");
			var watch = new Stopwatch();
			watch.Start();
			filter.Set(top, new PrebuiltPathFilter(scanner, type));

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"{watch.ElapsedMilliseconds}ms.\n");
			Console.ForegroundColor = ConsoleColor.Gray;
		}
		else
		{
			filter.Set(top, new CodingPathFilter(scanner, type));
		}

		var mountOptions = config.Mount;
		return new VirtualFS(filter, new()
		{
#if DEBUG
			Debug = true,
#else
			Debug = false,
#endif
			MountPoint = mountOptions.Point,
			Name = mountOptions.VolumeLabel,
			Readonly = mountOptions.Readonly,
		});
	}
}
