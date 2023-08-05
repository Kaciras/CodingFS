using CodingFS.Workspaces;
using Tomlyn;

namespace CodingFS.Cli;

public sealed class Config
{
	public string Root { get; set; } = Environment.CurrentDirectory;

	public bool Gitignore { get; set; }

	public int MaxDepth { get; set; } = int.MaxValue;

	public List<string> Deps { get; set; } = new();

	public List<string> Ingores { get; set; } = new();

	public CodingScanner CreateScanner()
	{
		GitWorkspace.Ignore = Gitignore;

		var custom = new CustomWorkspace();
		foreach (var module in Deps)
		{
			custom.Dict[module] = RecognizeType.Dependency;
		}
		foreach (var module in Ingores)
		{
			custom.Dict[module] = RecognizeType.Ignored;
		}

		var globals = new Workspace[] {
			custom,
			CodingScanner.GLOBALS[0]
		};
		return new(Root, globals) { MaxDepth = MaxDepth };
	}

	public static Config LoadToml(string? file)
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
