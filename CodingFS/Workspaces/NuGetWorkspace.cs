using System;
using System.IO;

namespace CodingFS.Workspaces;

public sealed class NuGetWorkspace : PackageManager
{
	static readonly string[] PACKAGES_CONFIG = { "packages.config" };
	static readonly string[] CACHE_STORE = { "packages" };

	readonly string? csproj;

	internal bool legacy;

	public string[] ConfigFiles => (legacy, Root == this) switch
	{
		(true, false) => PACKAGES_CONFIG,
		(true, true) => CACHE_STORE,
		(false, true) => new[] { csproj! },
		(false, false) => Array.Empty<string>(),
	};

	public PackageManager Root { get; }

	public NuGetWorkspace() 
	{
		Root = this;
	}

	public NuGetWorkspace(string csproj, NuGetWorkspace parent, bool legacy)
	{
		Root = parent;
		this.csproj = csproj;
		this.legacy = legacy;
	}

	public RecognizeType Recognize(string path)
	{
		if (Root != this || !legacy)
		{
			return RecognizeType.NotCare;
		}

		var folder = Path.GetDirectoryName(csproj.AsSpan());
		var relative = PathSpliter.GetRelative(folder, path);

		return relative.SequenceEqual("packages")
			? RecognizeType.Dependency : RecognizeType.NotCare;
	}
}
