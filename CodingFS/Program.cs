using System.Runtime.CompilerServices;
using CodingFS.Command;
using CodingFS.Workspaces;
using CommandLine;

[assembly: InternalsVisibleTo("CodingFS.Test")]
[assembly: InternalsVisibleTo("CodingFS.Benchmark")]
namespace CodingFS;

public interface CliCommand
{
	void Execute();
}

internal static class Program
{
	private static void Main(string[] args)
	{
		Parser.Default.ParseArguments<KeepGreen, Mount, Inspect>(args)
			.WithParsed<CliCommand>(command => command.Execute());
	}
}
