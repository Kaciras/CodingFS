using System;
using System.IO;

namespace CodingFS;

/// <summary>
/// ReadOnlyMemory&lt;char&gt; based path splitor, only support POSIX and DOS paths.
/// </summary>
public ref struct PathSpliter
{
	static int RelativePosition(ReadOnlySpan<char> path, ReadOnlySpan<char> relativeTo)
	{
		if (relativeTo.Length == 0) return -1;

		var end = relativeTo[^1];
		if (end == '\\' || end == '/')
		{
			relativeTo = relativeTo[..^1];
		}

		var length = relativeTo.Length;
		if (path.StartsWith(relativeTo))
		{
			if (path.Length == relativeTo.Length ||
				path[length] == '\\' ||
				path[length] == '/')
			{
				return length;
			}
		}
		throw new ArgumentException($"{path} is not relative to {relativeTo}");
	}

	/// <summary>
	/// ReadOnlySpan-based alternative of `Path.GetRelativePath`.
	/// Difference:
	/// 1) Comparison is case-sensitive on all platforms.
	/// 2) Throw ArgumentException if the paths don't share the same root.
	/// </summary>
	public static ReadOnlySpan<char> GetRelative(
		ReadOnlySpan<char> path,
		ReadOnlySpan<char> relativeTo)
	{
		var i = RelativePosition(path, relativeTo);
		return i == path.Length ? "." : path[(i + 1)..];
	}

	// =============================================================================

	readonly ReadOnlyMemory<char> path;
	readonly int root = -1;

	public int Index { get; set; } = -1;

	/// <summary>
	/// Create a new PathSplitor for the path, started at the end of the base path.
	/// </summary>
	/// <param name="path">The path to split</param>
	/// <param name="relativeTo">The base path to skip</param>
	/// <exception cref="ArgumentException">If the path is not relative to the base</exception>
	public PathSpliter(string path, ReadOnlySpan<char> relativeTo) : this(path)
	{
		Index = RelativePosition(path, relativeTo);
	}

	public PathSpliter(string path)
	{
		if (path.Length > 0 && path[0] == '/')
		{
			root = 0;
		}
		else if (path.Length > 2 && path[1] == ':')
		{
			root = 2;
		}
		this.path = path.AsMemory();
	}

	public readonly bool HasNext => Index != path.Length;

	public ReadOnlyMemory<char> SplitNext()
	{
		if (Index == -1 && root != -1)
		{
			Index = root;
			return path[..(root + 1)];
		}

		var slice = path[(Index + 1)..];
		var i = slice.Span.IndexOfAny('\\', '/');

		if (i == -1)
		{
			Index = path.Length;
			return slice;
		}
		else
		{
			Index += i + 1;
			return slice[..i];
		}
	}

	public readonly ReadOnlyMemory<char> Left
	{
		get => Index == root ? path[..(root + 1)] : path[..Index];
	}

	/// <summary>
	/// The slice after the current index, only available if there are remaining components.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">If HasNext == false</exception>
	public readonly ReadOnlyMemory<char> Right => path[(Index + 1)..];
}
