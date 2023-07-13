using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommandLine;

namespace CodingFS.Cli;

[Verb("list", HelpText = "Get the file list of specific type.")]
internal sealed class ListCommand : Command
{
	[Option('t', "type", HelpText = "What types of files should be included")]
	public FileType Type { get; set; }

	public void Execute()
	{
		Execute(@"D:\Coding");
	}

	private void Execute(string root)
	{
		var classifier = new CodingScanner(root);

		var groups = classifier.Walk(root, Type)
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
