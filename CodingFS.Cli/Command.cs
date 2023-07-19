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
		var options = new TomlModelOptions()
		{
			ConvertPropertyName = x => x,
		};
		var text = File.ReadAllText(file);
		return Toml.ToModel<Config>(text, file, options);
	}
}
