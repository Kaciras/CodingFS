using Tomlyn;

namespace CodingFS.Cli;

public interface Command
{
	void Execute();

	protected static Config LoadConfig(string? file)
	{
		if (file == null)
		{
			return new Config();
		}
		return Toml.ToModel<Config>(File.ReadAllText(file));
	}
}
