using CodingFS.Workspaces;

namespace CodingFS.Cli;

public sealed class Config
{
	public string Root { get; set; } = Environment.CurrentDirectory;

	public bool Gitignore { get; set; }

	public int ProjectDepth { get; set; } = int.MaxValue;

	public int ModuleDepth { get; set; } = int.MaxValue;

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
		return new CodingScanner(Root, globals)
		{
			ModuleDepth = ModuleDepth,
			ProjectDepth = ProjectDepth,
		};
	}
}
