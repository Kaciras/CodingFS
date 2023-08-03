using CommandLine;

namespace CodingFS.Cli;

[Verb("inspect", HelpText = "Show workspaces and their recognize result for path.")]
public sealed class InspectCommand : Command
{
	[Value(0, HelpText = "Config file to use.")]
	public string? ConfigFile { get; set; }

	[Value(1, HelpText = "Path to inspect.")]
	public string FileName { get; set; } = Environment.CurrentDirectory;

	public void Execute()
	{
		var scanner = Command.LoadConfig(ConfigFile).CreateScanner();
		var dir = FileName = Path.GetFullPath(FileName); 

		if (File.Exists(FileName))
		{
			dir = Path.GetDirectoryName(dir);
		}

		while (dir.Length > scanner.Root.Length)
		{
			var info = scanner.GetWorkspaces(dir);
			if(info.Current.Count > 0)
			{
				PrintRecognizeResults(info);
			}
			dir = Path.GetDirectoryName(dir);
		}
	}

	void PrintRecognizeResults(WorkspacesInfo info)
	{
		Console.WriteLine();
		Console.WriteLine(info.Directory);

		foreach (var w in info.Current)
		{
			Console.Write($"{w.Name} -> ");

			var recognized = w.Recognize(FileName);
			switch (recognized)
			{
				case RecognizeType.NotCare:
					Console.ForegroundColor = ConsoleColor.Blue;
					break;
				case RecognizeType.Dependency:
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					break;
				case RecognizeType.Ignored:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
			}
			Console.WriteLine(Enum.GetName(recognized));
			Console.ForegroundColor = ConsoleColor.White;
		}
	}
}
