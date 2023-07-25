using System;

namespace CodingFS;

/// <summary>
/// ReadOnlyMemory&lt;char&gt; based path splitor, only support POSIX and DOS paths.
/// </summary>
public ref struct PathSpliter
{
	readonly ReadOnlyMemory<char> path;
	readonly int root = -1;

	public int Index { get; set; } = -1;

	public PathSpliter(string path, string relativeTo) : this(path)
	{
		if (relativeTo.Length == 0) return;

		if (relativeTo[^1] == '\\' || relativeTo[^1] == '/')
		{
			relativeTo = relativeTo[..^1];
		}

		var length = relativeTo.Length;
		var span = path.AsSpan();

		if (span.StartsWith(relativeTo))
		{
			if (span.Length == relativeTo.Length ||
				span[length] == '\\' ||
				span[length] == '/')
			{
				Index = length;
				return;
			}
		}
		throw new ArgumentException($"{path} is not relative to {relativeTo}");
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
