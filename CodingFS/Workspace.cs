using System;
using System.Collections.Generic;

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
	string[] ConfigFiles { get; }

	PackageManager? Parent { get; }
}
