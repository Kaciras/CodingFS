using System.Collections.Generic;
using System.Linq;

namespace CodingFS;

public readonly struct WorkspacesInfo
{
	public IReadOnlyList<Workspace> Workspaces { get; }

	public IReadOnlyList<Workspace> Current { get; }

	public string Directory { get; }

	internal WorkspacesInfo(
		string path,
		IReadOnlyList<Workspace> workspaces,
		IReadOnlyList<Workspace> current)
	{
		Directory = path;
		Workspaces = workspaces;
		Current = current;
	}

	public IEnumerable<T> FindType<T>() where T : Workspace
	{
		return Workspaces.OfType<T>();
	}

	public FileType GetFileType(string path)
	{
		var recognized = RecognizeType.NotCare;
		foreach (var w in Workspaces)
		{
			recognized |= w.Recognize(path);
		}
		return recognized.ToFileType();
	}
}
