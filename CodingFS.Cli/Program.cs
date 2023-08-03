using System.Runtime.CompilerServices;
using CodingFS.Cli;
using CommandLine;

[module: SkipLocalsInit]

var parser = new Parser(options =>
{
	options.CaseInsensitiveEnumValues = true;
	options.AutoHelp = true;
	options.AutoVersion = true;
	options.HelpWriter = Console.Error;
});

parser.ParseArguments<MountCommand, ListCommand, InspectCommand>(args)
	.WithParsed<Command>(command => command.Execute());
