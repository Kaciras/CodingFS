using System;
using System.Collections.Generic;

namespace CodingFS;

public enum WorkspaceKind : byte { Other, PM, IDE, VCS }

public interface Workspace
{
	ReadOnlySpan<char> Name
	{
		get => GetType().Name.AsSpan().TrimEnd("Workspace");
	}

	WorkspaceKind Kind { get; }

	RecognizeType Recognize(string relativePath);
}
