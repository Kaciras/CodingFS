using CommandLine;

namespace CodingFS.Cli;

public abstract class Command
{
	[Option('c', "config", HelpText = "Path of the config file to use.")]
	public string? ConfigFile { get; set; }

	public void Execute()
	{
		Execute(Config.LoadToml(ConfigFile));
	}

	protected abstract void Execute(Config config);
}
