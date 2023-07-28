using System;
using System.IO;
using System.Linq;
using System.Text;
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
		if (Utils.IsDir(ctx.Path, ".idea"))
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
	public string? ExternalBuildSystem(string path)
	{
		if (localConfig == null) return null;

		var hash = Utils.JavaStringHashCode(path.Replace('\\', '/')).ToString("x2");

		var builder = new StringBuilder()
			.Append(localConfig)
			.Append(Path.DirectorySeparatorChar)
			.Append("projects")
			.Append(Path.DirectorySeparatorChar)
			.Append(Path.GetFileName(path.AsSpan()))
			.Append('.')
			.Append(hash)
			.Append(Path.DirectorySeparatorChar)
			.Append("external_build_system")
			.Append(Path.DirectorySeparatorChar)
			.Append("modules");

		var modules = builder.ToString();
		return Directory.Exists(modules) ? modules : null;
	}
}
