using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SpecialFolder = System.Environment.SpecialFolder;

namespace CodingFS.Workspaces;

public sealed partial class JetBrainsDetector
{
	[GeneratedRegex("^.+IntelliJIdea(20[0-9.]+)$")]
	private static partial Regex JBConfigRE();

	// Config directory of the latest version of IntelliJIdea.
	private string? localConfig;

	public JetBrainsDetector()
	{
		var home = Environment.GetFolderPath(SpecialFolder.LocalApplicationData);

		localConfig = Directory
			.EnumerateDirectories(Path.Join(home, "JetBrains"))
			.Select(path => JBConfigRE().Match(path))
			.Where(match => match.Success)
			.MaxBy(match => Version.Parse(match.Groups[1].ValueSpan))?.Value;
	}

	public void Detect(DetectContxt ctx)
	{
		if (Directory.Exists(Path.Combine(ctx.Path, ".idea")))
		{
			ctx.AddWorkspace(new IDEAWorkspace(this, ctx.Path));
		}
	}

	/// <summary>
	/// There iml files in [localConfig]/projects/[NAME]/external_build_system/modules.
	/// <br/>
	/// How to get the [NAME] of the project：
	/// https://github.com/JetBrains/intellij-community/blob/734efbef5b75dfda517731ca39fb404404fbe182/platform/platform-api/src/com/intellij/openapi/project/ProjectUtil.kt#L146
	/// </summary>
	public string? EBSModuleFiles(string path)
	{
		if (localConfig == null) return null;

		var cache = Utils.JavaStringHashcode(path.Replace('\\', '/')).ToString("x2");
		cache = Path.GetFileName(path) + "." + cache;
		var modules = Path.Join(localConfig, "projects", cache, "external_build_system/modules");

		return Directory.Exists(modules) ? modules : null;
	}
}
