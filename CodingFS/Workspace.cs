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

	RecognizeType Recognize(string file);
}

public readonly struct DetectContxt
{
	public readonly IReadOnlyList<Workspace> Parent;
	public readonly string Path;

	readonly List<Workspace> Matches;

	public DetectContxt(
		IReadOnlyList<Workspace> parent,
		string path,
		List<Workspace> matches)
	{
		Parent = parent;
		Path = path;
		Matches = matches;
	}

	public void Deconstruct(
		out string path,
		out IReadOnlyList<Workspace> parent)
	{
		path = Path;
		parent = Parent;
	}

	public void AddWorkspace(Workspace value) => Matches.Add(value);
}

public delegate void WorkspaceFactory(DetectContxt ctx);
