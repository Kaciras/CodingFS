using CommandLine;
using System.Drawing;
using System.Windows.Input;

namespace CodingFS.Cli;

internal class Program
{
	static void Main(string[] args)
	{
		Parser.Default.ParseArguments<KeepGreen, Mount, Inspect>(args)
			.WithParsed<Command>(command => command.Execute());
	}
}
