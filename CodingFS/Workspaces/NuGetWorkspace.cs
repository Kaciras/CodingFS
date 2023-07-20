using System;

namespace CodingFS.Workspaces;

public sealed class NuGetWorkspace : PackageManager
{
	static readonly string[] PACKAGES_CONFIG = { "packages.config" };
	static readonly string[] CACHE_STORE = { "packages" };

	readonly string? csproj;

	internal bool legacy;

	public WorkspaceKind Kind => WorkspaceKind.PM;

	public string[] ConfigFiles => (legacy, Parent == null) switch
	{
		(true, false) => PACKAGES_CONFIG,
		(true, true) => CACHE_STORE,
		(false, true) => new[] { csproj! },
		(false, false) => Array.Empty<string>(),
	};

	public PackageManager? Parent { get; }

	public NuGetWorkspace() {}

	public NuGetWorkspace(string csproj, NuGetWorkspace parent, bool legacy)
	{
		Parent = parent;
		this.csproj = csproj;
		this.legacy = legacy;
	}

	public RecognizeType Recognize(string path)
	{
		return path == "packages" && Parent == null && legacy
			? RecognizeType.Dependency : RecognizeType.NotCare;
	}
}
