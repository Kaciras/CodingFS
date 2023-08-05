using CommandLine;

namespace CodingFS.Cli;

[Verb("inspect", HelpText = "Show workspaces and their recognize result for path.")]
public sealed class InspectCommand : Command
{
	[Value(0, HelpText = "Path to inspect.")]
	public string FileName { get; set; } = Environment.CurrentDirectory;

	protected override void Execute(Config config)
	{
		FileName = Path.GetFullPath(FileName);
		var dir = Path.GetDirectoryName(FileName)!;

		var scanner = config.CreateScanner();
		var info = scanner.GetWorkspaces(dir);
		var type = info.GetFileType(FileName);

		Console.Write($"File type of {FileName} is ");
		Console.ForegroundColor = GetColor(type);
		Console.WriteLine(Enum.GetName(type));
		Console.ForegroundColor = ConsoleColor.Gray;

		var splitor = new PathSpliter(dir, scanner.Root);
		while (splitor.HasNext)
		{
			splitor.SplitNext();

			info = scanner.GetWorkspaces(splitor.Left.ToString());
			if (info.Current.Count > 0)
			{
				PrintRecognizeResults(info);
			}
		}
	}

	void PrintRecognizeResults(WorkspacesInfo info)
	{
		Console.WriteLine();
		Console.WriteLine(info.Directory);

		foreach (var w in info.Current)
		{
			if (w is PackageManager p && p.Root != p)
			{
				Console.Write($"{w.Name} [Sub] -> ");
			}
			else
			{
				Console.Write($"{w.Name} -> ");
			}

			var type = w.Recognize(FileName);
			Console.ForegroundColor = GetColor(type);
			Console.WriteLine(Enum.GetName(type));
			Console.ForegroundColor = ConsoleColor.Gray;
		}
	}

	static ConsoleColor GetColor(FileType x) => x switch
	{
		FileType.Source => ConsoleColor.Blue,
		FileType.Dependency => ConsoleColor.DarkYellow,
		FileType.Generated => ConsoleColor.Red,
		_ => throw new NotImplementedException(Enum.GetName(x)),
	};

	static ConsoleColor GetColor(RecognizeType x) => x switch
	{
		RecognizeType.NotCare => ConsoleColor.DarkGreen,
		RecognizeType.Dependency => ConsoleColor.DarkYellow,
		RecognizeType.Ignored => ConsoleColor.Red,
		_ => throw new NotImplementedException(Enum.GetName(x)),
	};
}
