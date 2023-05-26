using System.Runtime.CompilerServices;
using CodingFS.Command;
using CommandLine;

[assembly: InternalsVisibleTo("CodingFS.Test")]
[assembly: InternalsVisibleTo("CodingFS.Benchmark")]
namespace CodingFS;

internal static class Program
{
	private static void Main(string[] args)
	{
		Parser.Default.ParseArguments<KeepGreen, Mount, Inspect>(args)
			.WithParsed<CliCommand>(command => command.Execute());
	}
}
