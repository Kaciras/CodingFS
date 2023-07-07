using CodingFS.Cli;
using CommandLine;
using System.Runtime.CompilerServices;

[module: SkipLocalsInit]

Parser.Default
	.ParseArguments<MountCommand, ListCommand>(args)
	.WithParsed<Command>(command => command.Execute());
