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

	public static Workspace? Match(List<Workspace> ancestor, string path)
	{
		if (!File.Exists(Path.Combine(path, "package.json")))
		{
			return null;
		}

		// 
		var parent = ancestor.OfType<NpmWorkspace>().FirstOrDefault();

		var packageManager = "npm";
		if(parent != null)
		{
			packageManager = parent.PackageManager;
		}
		if (!File.Exists(Path.Combine(path, "pnpm-lock.yaml")))
		{
			packageManager = "pnpm";
		}

		return new NpmWorkspace(parent, path, packageManager);
	}
}
