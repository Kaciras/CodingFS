using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CodingFS.Helper;
using SpecialFolder = System.Environment.SpecialFolder;

namespace CodingFS.Workspaces;

public partial class JetBrainsDetector
{
	[GeneratedRegex("^.+IntelliJIdea(20[0-9.]+)$")]
	private static partial Regex JBConfigRE();

	// Config directory of the latest version of IntelliJIdea.
	readonly string? localConfig;

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
	public virtual string? ExternalBuildSystem(string path)
	{
		if (localConfig == null)
		{
			return null;
		}
		path = path.Replace('\\', '/');
		var hash = Utils.JavaStringHashCode(path);

		var builder = new StackStringBuilder(stackalloc char[4096]);
		builder.Append(localConfig);
		builder.Append(Path.DirectorySeparatorChar);
		builder.Append("projects");
		builder.Append(Path.DirectorySeparatorChar);
		builder.Append(Path.GetFileName(path.AsSpan()));
		builder.Append('.');
		builder.AppendFormat(hash, "x2");
		builder.Append(Path.DirectorySeparatorChar);
		builder.Append("external_build_system");
		builder.Append(Path.DirectorySeparatorChar);

		return builder.ToString();
	}
}
