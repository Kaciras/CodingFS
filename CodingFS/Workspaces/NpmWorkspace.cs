using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodingFS.Workspaces;

public class NpmWorkspace : Workspace
{
	public NpmWorkspace? Parent { get; }

	public string Directory { get; }

	public string PackageManager { get; }

	public NpmWorkspace(NpmWorkspace? parent, string directory, string packageManager)
	{
		Directory = directory;
		Parent = parent;
		PackageManager = packageManager;
	}

	public RecognizeType Recognize(string path)
	{
		if (Path.GetFileName(path) == "node_modules")
		{
			return RecognizeType.Dependency;
		}
		return RecognizeType.NotCare;
	}

	public static void Match(DetectContxt ctx)
	{
		if (!File.Exists(Path.Combine(ctx.Path, "package.json")))
		{
			return;
		}

		// 
		var parent = ctx.Parent.OfType<NpmWorkspace>().FirstOrDefault();
		var type = "npm";

		if (parent != null)
		{
			type = parent.PackageManager;
		}
		if (!File.Exists(Path.Combine(ctx.Path, "pnpm-lock.yaml")))
		{
			type = "pnpm";
		}

		ctx.AddWorkspace(new NpmWorkspace(parent, ctx.Path, type));
	}
}
