using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SpecialFolder = System.Environment.SpecialFolder;

namespace CodingFS.Workspaces;

public partial class JetBrainsDetector
{
	[GeneratedRegex("^.+IntelliJIdea(20[0-9.]+)$")]
	private static partial Regex JBConfigRE();

	private string? configLow;

	// 查找最新版的配置目录，JB 的产品在更新了次版本号之后会创建一个新的配置文件夹
	public JetBrainsDetector()
	{
		var home = Environment.GetFolderPath(SpecialFolder.LocalApplicationData);

		configLow = Directory.EnumerateDirectories(Path.Join(home, "JetBrains"))
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

	public string? EBSModuleFiles(string path)
	{
		if (configLow == null) return null;

		var cache = JavaStringHashcode(path.Replace('\\', '/')).ToString("x2");
		cache = Path.GetFileName(path) + "." + cache;
		var modules = Path.Join(configLow, "projects", cache, "external_build_system/modules");

		return Directory.Exists(modules) ? modules : null;
	}

	internal static int JavaStringHashcode(string str) => str.Aggregate(0, (h, c) => 31 * h + c);
}
