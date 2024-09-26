using System;
using CodingFS.Helper;

namespace CodingFS.Workspaces;

public sealed class NuGetWorkspace : PackageManager
{
	static readonly string[] PACKAGES_CONFIG = ["packages.config"];

	// For virtial root, it's the solution folder, else is .csproj file.
	readonly string path;

	readonly bool legacy;

	public string[] ConfigFiles => (Root == this, legacy) switch
	{
		(true, _) => [],
		(false, false) => [path],
		(false, true) => PACKAGES_CONFIG,
	};

	// If Root == this, it is the vritual workspace root.
	public PackageManager Root { get; }

	public NuGetWorkspace(string path)
	{
		Root = this;
		this.path = path;
	}

	public NuGetWorkspace(string csproj, NuGetWorkspace parent, bool legacy)
	{
		Root = parent;
		path = csproj;
		this.legacy = legacy;
	}

	public RecognizeType Recognize(string path)
	{
		if (Root != this)
		{
			return RecognizeType.NotCare;
		}

		var relative = PathSpliter.GetRelative(this.path, path);
		return relative.SequenceEqual("packages")
			? RecognizeType.Dependency : RecognizeType.NotCare;
	}
}
