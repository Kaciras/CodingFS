using CodingFS.Workspaces;
using Tomlyn;

namespace CodingFS.Cli;

public sealed class Config
{
	public string Root { get; set; } = Environment.CurrentDirectory;

	public BuiltinDetectorOptions Detector { get; set; }

	public int MaxDepth { get; set; } = int.MaxValue;

	public List<string> Deps { get; set; } = [];

	public List<string> Ingores { get; set; } = [];

	public CodingScanner CreateScanner()
	{
		var detectors = Detectors.GetBuiltins(Detector);

		var custom = new Dictionary<string, RecognizeType>();
		foreach (var module in Deps)
		{
			custom[module] = RecognizeType.Dependency;
		}
		foreach (var module in Ingores)
		{
			custom[module] = RecognizeType.Ignored;
		}

		var globals = new Workspace[] {
			new CustomWorkspace("Custom", custom),
			new CommonWorkspace(),
		};
		return new(Root, globals, detectors) { MaxDepth = MaxDepth };
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
