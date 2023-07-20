using System;
using System.IO;
using System.Linq;

namespace CodingFS.Workspaces;

public sealed class NpmWorkspace : PackageManager
{
	static readonly string[] NPM_CONFIGS = { "package.json", "package-lock.json" };

	public WorkspaceKind Kind => WorkspaceKind.PM;

	public string Directory { get; }

	public string App { get; }

	public string[] ConfigFiles { get; }

	public PackageManager? Parent { get; }

	NpmWorkspace(string directory, string app, string[] configFiles, NpmWorkspace? parent)
	{
		Directory = directory;
		App = app;
		ConfigFiles = configFiles;
		Parent = parent;
	}

	public RecognizeType Recognize(string path)
	{
		var name = Path.GetFileName(path.AsSpan());
		return name.SequenceEqual("node_modules") 
			? RecognizeType.Dependency : RecognizeType.NotCare;
	}

	public static void Match(DetectContxt ctx)
	{
		var (path, parents) = ctx;

		if (!Utils.IsFile(path, "package.json"))
		{
			return;
		}

		var parent = parents.OfType<NpmWorkspace>().FirstOrDefault();
		var app = "npm";
		var files = NPM_CONFIGS;

		if (parent != null)
		{
			app = parent.App;
			files = new[] { "package.json" };
		}
		else if (Utils.IsFile(path, "pnpm-lock.yaml"))
		{
			app = "pnpm";
			files = new[] { "package.json", "pnpm-lock.yaml" };
		}
		else if (Utils.IsFile(path, "yarn.lock"))
		{
			app = "yarn";
			files = new[] { "package.json", "yarn.lock" };
		}

		ctx.AddWorkspace(new NpmWorkspace(path, app, files, parent));
	}
}
