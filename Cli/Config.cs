using CodingFS.Workspaces;
using Tomlyn;

namespace CodingFS.Cli;

public sealed class Config
{
	public const string DEFAULT_CONFIG_FILE = "config.toml";

	public string Root { get; set; } = Environment.CurrentDirectory;

	public int MaxDepth { get; set; } = int.MaxValue;

	public List<string> Deps { get; set; } = [];

	public List<string> Ingores { get; set; } = [];

	public MountOptions Mount { get; set; } = new();

	public BuiltinDetectorOptions Detector { get; set; } = new();

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
			Workspace.FromFn(custom.GetValueOrDefault),
			new CommonWorkspace(),
		};
		return new(Root, globals, detectors) { MaxDepth = MaxDepth };
	}

	/// <summary>
	/// Load config from file, it must exists, default is config.toml.
	/// </summary>
	public static Config LoadToml(string? file)
	{
		var options = new TomlModelOptions()
		{
			ConvertPropertyName = x => x,
		};
		file ??= DEFAULT_CONFIG_FILE;
		var text = File.ReadAllText(file);
		return Toml.ToModel<Config>(text, file, options);
	}

	public void SaveToml(string file)
	{
		var options = new TomlModelOptions()
		{
			ConvertPropertyName = x => x,
		};
		File.WriteAllText(file, Toml.FromModel(this, options));
	}
}
