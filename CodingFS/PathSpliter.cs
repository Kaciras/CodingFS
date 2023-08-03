using System;
using System.IO;

namespace CodingFS;

/// <summary>
/// ReadOnlyMemory&lt;char&gt; based path splitor. Only support POSIX and DOS paths.
/// </summary>
public ref struct PathSpliter
{
	static int RelativePosition(ReadOnlySpan<char> path, ReadOnlySpan<char> relativeTo)
	{
		//switch (Path.IsPathRooted(path), Path.IsPathRooted(relativeTo))
		//{
		//	case (true, false):
		//		TrimCwd(ref path, ref relativeTo);
		//		break;
		//	case (false, true):
		//		TrimCwd(ref relativeTo, ref path);
		//		break;
		//}

        if (relativeTo.Length == 0) return -1;

		if (IsSep(relativeTo[^1]))
		{
			relativeTo = relativeTo[..^1];
		}

		if (path.StartsWith(relativeTo))
		{
			var length = relativeTo.Length;
			if (path.Length == relativeTo.Length || IsSep(path[length]))
			{
				return length;
			}
		}
		throw new ArgumentException($"{path} is not relative to {relativeTo}");
	}

	static void TrimCwd(
		ref ReadOnlySpan<char> absoulate, 
		ref ReadOnlySpan<char> relative)
	{
		var cwd = Environment.CurrentDirectory.AsSpan();

		while (relative.Length > 0 && relative[0] == '.')
		{
			if (relative.Length == 1) // "."
			{
				relative = ReadOnlySpan<char>.Empty;
				break;
			}
			else if (relative[1] == '.'
				&& relative.Length > 2
				&& IsSep(relative[2]))
			{
				relative = relative[3..];
				cwd = Path.GetDirectoryName(cwd);
			}
			else
			{
				break;
			}
		}

		absoulate = GetRelative(cwd, absoulate);
	}

	static bool IsSep(char @char) => @char == '\\' || @char == '/';

	/// <summary>
	/// ReadOnlySpan-based alternative of `Path.GetRelativePath`.
	/// Difference:
	/// 1) Comparison is case-sensitive on all platforms.
	/// 2) Throw ArgumentException if the paths don't share the same root.
	/// </summary>
	public static ReadOnlySpan<char> GetRelative(
		ReadOnlySpan<char> relativeTo,
		ReadOnlySpan<char> path)
	{
		var i = RelativePosition(path, relativeTo);
		return i == path.Length ? "." : path[(i + 1)..];
	}

	// =============================================================================

	readonly ReadOnlyMemory<char> path;
	readonly int root;

	public int Index { get; set; } = -1;

	public PathSpliter(string path, string relativeTo) : this(path.AsMemory(), relativeTo) {}

	public PathSpliter(string path) : this(path.AsMemory()) { }

	/// <summary>
	/// Create a new PathSplitor for the path, started at the end of the base path.
	/// </summary>
	/// <param name="path">The path to split</param>
	/// <param name="relativeTo">The base path to skip</param>
	/// <exception cref="ArgumentException">If the path is not relative to the base</exception>
	public PathSpliter(ReadOnlyMemory<char> path, ReadOnlySpan<char> relativeTo) : this(path)
	{
		Index = RelativePosition(path.Span, relativeTo);
	}

	public PathSpliter(ReadOnlyMemory<char> path)
	{
		this.path = path;

		root = path.Span switch
		{
			['/', ..] => 0,         // POSIX absoulate, e.g. "/foo/bar"
			[_, ':', _, ..] => 2,   // DOS absoulate, e.g. "C:\foo\bar"
			_ => -1,                // Relative, we do't support other types.
		};
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
