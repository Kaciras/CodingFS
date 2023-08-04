using System;

namespace CodingFS;

[Flags]
public enum WorkspaceKind : byte { 
	Other, 
	PM, 
	IDE,
	VCS = 4,
}

public interface Workspace
{
	ReadOnlySpan<char> Name
	{
		get => GetType().Name.AsSpan().TrimEnd("Workspace");
	}

	WorkspaceKind Kind { get; }

	RecognizeType Recognize(string relativePath);
}

public interface PackageManager : Workspace
{
	/// <summary>
	/// Return `this` if the instance is top-level, otherwise return the 
	/// package manager on the project root.
	/// </summary>
	PackageManager Root { get; }

	/// <summary>
	/// Files that contains metadata that is needed to compile the package.
	/// Usually manifest file and lock file (e.g. "cargo.toml" and "cargo.lock").
	/// 
	/// It not ensure these files are exists, you may neeed to check that.
	/// </summary>
	string[] ConfigFiles { get; }
}
