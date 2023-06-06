using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CodingFS;

internal ref struct PathComponentSpliter
{
	readonly ReadOnlyMemory<char> path;

	int index = -1;

	public PathComponentSpliter(string path)
	{
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

	public readonly ReadOnlyMemory<char> Left => path[..index];

	public readonly ReadOnlyMemory<char> Right => path[(index + 1)..];
}
