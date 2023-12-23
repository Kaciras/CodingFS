using CommandLine;

namespace CodingFS.Cli;

[Verb("inspect", HelpText = "Show workspaces and their recognize result for the specific path. Useful for debug.")]
public sealed class InspectCommand : Command
{
	[Value(0, HelpText = "Path to inspect.")]
	public string FileName { get; set; } = Environment.CurrentDirectory;

	static void WriteColored(string? text, ConsoleColor color, bool lf = true)
	{
		Console.ForegroundColor = color;
		Console.Write(text);
		if (lf)
		{
			Console.WriteLine();
		}
		Console.ForegroundColor = ConsoleColor.Gray;
	}

	protected override void Execute(Config config)
	{
		FileName = Path.GetFullPath(FileName);
		var dir = Path.GetDirectoryName(FileName)!;

		var scanner = config.CreateScanner();
		var info = scanner.GetWorkspaces(dir);
		var type = info.GetFileType(FileName);

		Console.Write("File type of ");
		WriteColored(FileName, ConsoleColor.Cyan, false);
		Console.Write(" is ");
		WriteColored(Enum.GetName(type), GetColor(type));

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
			WriteColored(Enum.GetName(type), GetColor(type));
		}
	}

	static ConsoleColor GetColor(FileType x) => x switch
	{
		FileType.Source => ConsoleColor.DarkGreen,
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
