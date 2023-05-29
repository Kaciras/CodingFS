using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodingFS.Workspaces;

public partial class JetBrainsDetector : WorkspaceDetector
{
	[GeneratedRegex("^.+IntelliJIdea(20[0-9.]+)$")]
	private static partial Regex JBConfigRE();

	private string? ideaConfigLow;

	// 查找最新版的配置目录，JB的产品在更新了次版本号之后会创建一个新的配置文件夹
	public JetBrainsDetector()
	{
		var IDEA_DIR_RE = JBConfigRE();
		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

		var latest = Directory.EnumerateDirectories(Path.Join(home, "JetBrains"))
			.Select(path => IDEA_DIR_RE.Match(path))
			.Where(match => match.Success)
			.MaxBy(match => Version.Parse(match.Groups[1].ValueSpan));

		ideaConfigLow = latest?.Value;
	}

	public Workspace? Detect(string path)
	{
		return Directory.Exists(Path.Combine(path, ".idea")) ? new IDEAWorkspace(this, path) : null;
	}

	public string? EBSModuleFiles(string path)
	{
		if (ideaConfigLow == null) return null;

		var cache = JavaStringHashcode(path).ToString("x2");
		cache = Path.GetFileName(path) + "." + cache;
		var modules = Path.Join(ideaConfigLow, "projects", cache, "external_build_system/modules");

		return Directory.Exists(modules) ? modules : null;
	}

	internal static int JavaStringHashcode(string str) => str.Aggregate(0, (h, c) => 31 * h + c);
}
