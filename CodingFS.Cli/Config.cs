using System.Collections.Immutable;
using CodingFS.Workspaces;

namespace CodingFS.Cli;

public sealed class Config
{
	public string Root { get; set; } = Environment.CurrentDirectory;

	public bool Gitignore { get; set; }

	public int ProjectDepth { get; set; } = int.MaxValue;

	public int ModuleDepth { get; set; } = int.MaxValue;

	public IList<string> Deps { get; set; } = ImmutableList.Create<string>();

	public IList<string> Ingores { get; set; } = ImmutableList.Create<string>();

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
		return new CodingScanner(Root, globals, CodingScanner.DETECTORS)
		{
			ModuleDepth = ModuleDepth,
			ProjectDepth = ProjectDepth,
		};
	}
}
