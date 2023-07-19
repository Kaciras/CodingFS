using System.Collections.Immutable;
using CommandLine;

namespace CodingFS.Cli;

[Verb("list", HelpText = "Get the file list of specific type.")]
internal sealed class ListCommand : Command
{
	[Value(0, HelpText = "Config file to use.")]
	public string? ConfigFile { get; set; }

	[Option('t', "type", HelpText = "What types of files should be included")]
	public FileType Type { get; set; }

	public void Execute()
	{
		var config = Command.LoadConfig(ConfigFile);
		var scanner = config.CreateScanner();

		var groups = scanner.Walk(config.Root, Type)
			.GroupBy(v => v.Item2, v => v.Item1)
			.ToImmutableDictionary(i => i.Key);

		static void PrintGroup(IEnumerable<string> files, ConsoleColor color, string header)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(header);
			foreach (var file in files) Console.WriteLine(file);
			Console.ResetColor();
		}

		PrintGroup(groups[FileType.Dependency], ConsoleColor.Blue, "Dependencies:");
		PrintGroup(groups[FileType.Generated], ConsoleColor.Red, "Generated files:");
	}
}
