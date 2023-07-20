using System.Collections.Generic;

namespace CodingFS;

public delegate void Detector(DetectContxt context);

public readonly struct DetectContxt
{
	public readonly IReadOnlyList<Workspace> Parent;
	public readonly string Path;

	readonly List<Workspace> matches;

	public DetectContxt(
		IReadOnlyList<Workspace> parent,
		string path,
		List<Workspace> matches)
	{
		Parent = parent;
		Path = path;
		this.matches = matches;
	}

	public void Deconstruct(
		out string path,
		out IReadOnlyList<Workspace> parent)
	{
		path = Path;
		parent = Parent;
	}

	public void AddWorkspace(Workspace value) => matches.Add(value);
}
