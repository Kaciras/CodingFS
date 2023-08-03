using System.Collections.Immutable;
using CommandLine;

namespace CodingFS.Cli;

[Verb("list", HelpText = "Get the file list of specific type.")]
public sealed class ListCommand : Command
{
	[Value(0, HelpText = "Config file to use.")]
	public string? ConfigFile { get; set; }

	[Option('t', "type", HelpText = "What types of files should be included.")]
	public FileType Type { get; set; }

	[Option('n', "name-only", HelpText = "Only show file names.")]
	public bool NameOnly { get; set; }
	
	public void Execute()
	{
		var config = Command.LoadConfig(ConfigFile);
		var scanner = config.CreateScanner();
		var walking = scanner.Walk(config.Root, Type);

		if (NameOnly)
		{
			foreach (var (name, _) in walking)
			{
				Console.WriteLine(name);
			}
			return;
		}

		var groups = walking
			.GroupBy(v => v.Item2, v => v.Item1.FullName)
			.ToImmutableDictionary(i => i.Key);

		PrintGroup(groups[FileType.Dependency], ConsoleColor.Blue, "Dependencies:");
		PrintGroup(groups[FileType.Generated], ConsoleColor.Red, "Generated files:");
	}

	static void PrintGroup(IEnumerable<string> files, ConsoleColor color, string header)
	{
		Console.ForegroundColor = color;
		Console.WriteLine(header);
		foreach (var file in files) Console.WriteLine(file);
	}
}
