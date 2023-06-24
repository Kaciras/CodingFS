using System;
using System.Runtime.InteropServices;

namespace CodingFS;

// Only support POSIX and DOS paths.
internal ref struct PathComponentSpliter
{
	readonly int root = -1;
	readonly ReadOnlyMemory<char> path;

	int index = -1;

	public PathComponentSpliter(string path)
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

	public readonly void NormalizeSepUnsafe()
	{
		var span = MemoryMarshal.AsMemory(path).Span;
		for (int i = 0; i < span.Length; i++)
		{
			if (span[i] == '\\') span[i] = '/';
		}
	}

	public void Relative(ReadOnlySpan<char> root)
	{
		if (root.IsEmpty) return;
		if (root[^1] == '/') root = root[..^1];

		var length = root.Length;
		var span = path.Span;

		if (span.Length > root.Length &&
			span[..length].SequenceEqual(root) &&
			span[length] == '/')
		{
			index = length;
			return;
		}
		throw new ArgumentException($"{path} is not relative to {root}");
	}

	public readonly bool HasNext => index != path.Length;

	public ReadOnlyMemory<char> SplitNext()
	{
		if (index == -1 && root != -1)
		{
			index = root;
			return path[..(root + 1)];
		}

		var slice = path[(index + 1)..];
		var i = slice.Span.IndexOf('/');

		if (i == -1)
		{
			index = path.Length;
			return slice;
		}
		else
		{
			index += i + 1;
			return slice[..i];
		}
	}

	public readonly ReadOnlyMemory<char> Left
	{
		get
		{
			return index == root ? path[..(root + 1)] : path[..index];
		}
	}

	public readonly ReadOnlyMemory<char> Right => path[(index + 1)..];
}
