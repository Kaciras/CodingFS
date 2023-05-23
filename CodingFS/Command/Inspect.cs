using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine.Text;
using CommandLine;

namespace CodingFS.Command;

[Verb("inspect", HelpText = "在控制台打印出各种分类的文件")]
internal sealed class Inspect : CliCommand
{
	[Option('s', "source", HelpText = "打印源文件（可能很多）")]
	public bool Source { get; set; }

	public void Execute()
	{
		Execute(@"D:\Coding");
	}

	private void Execute(string root)
	{
		var classifier = new RootFileClassifier(root);

		static void PrintGroup(IEnumerable<string> files, ConsoleColor color, string header)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(header);
			foreach (var file in files) Console.WriteLine(file);
			Console.ResetColor();
		}

		var groups = classifier.Group();
		PrintGroup(groups[FileType.Dependency], ConsoleColor.Blue, "Dependencies:");
		PrintGroup(groups[FileType.Generated], ConsoleColor.Red, "Generated files:");
	}
}
