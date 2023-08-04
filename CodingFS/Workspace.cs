using System;

namespace CodingFS;

/// <summary>
/// Represents the configuration of a folder structure, which can be IDE, VCS or package manager.
/// </summary>
public interface Workspace
{
	ReadOnlySpan<char> Name
	{
		get => GetType().Name.AsSpan().TrimEnd("Workspace");
	}

	RecognizeType Recognize(string absoulatePath);
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
	/// <br/>
	/// It does not ensure these files are exists, you may neeed to check that.
	/// </summary>
	string[] ConfigFiles { get; }
}
