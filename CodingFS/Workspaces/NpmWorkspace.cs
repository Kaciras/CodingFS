using System.IO;

namespace CodingFS.Workspaces;

public class NpmWorkspace : Workspace
{
	public string PackageManager { get; }

	readonly string root;

	public NpmWorkspace(string root, string packageManager)
	{
		this.root = root;
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

	public static Workspace? Match(string path)
	{
		if (!File.Exists(Path.Combine(path, "package.json")))
		{
			return null;
		}

		var packageManager = "npm";
		if (!File.Exists(Path.Combine(path, "pnpm-lock.yaml")))
		{
			packageManager = "pnpm";
		}

		return new NpmWorkspace(path, packageManager);
	}
}
