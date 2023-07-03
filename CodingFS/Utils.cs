using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("CodingFS.Test")]
[assembly: InternalsVisibleTo("CodingFS.Benchmark")]
[module: SkipLocalsInit]

namespace CodingFS;

static class Utils
{
	/// <summary>
	/// Check ReadOnlyMemory&lt;char&gt; equality by their content.
	/// </summary>
	public static readonly CharMemComparator memComparator = new();

	/// <summary>
	/// Judging the file type by the classification result of workspaces.
	/// </summary>
	public static FileType ToFileType(this RecognizeType flags)
	{
		if (flags == RecognizeType.NotCare)
		{
			return FileType.Source;
		}
		if ((flags & RecognizeType.Dependency) == 0)
		{
			return FileType.Generated;
		}
		else
		{
			return FileType.Dependency;
		}
	}

	public static int JavaStringHashcode(ReadOnlySpan<char> str)
	{
		var hashCode = 0;
		for (var i = 0; i < str.Length; i++)
		{
			hashCode = 31 * hashCode + str[i];
		}
		return hashCode;
	}

	public static void NormalizeSepUnsafe(string path)
	{
		NormalizeSepUnsafe(path.AsMemory());
	}

	public static void NormalizeSepUnsafe(ReadOnlyMemory<char> path)
	{
		var span = MemoryMarshal.AsMemory(path).Span;
		for (var i = 0; i < span.Length; i++)
		{
			if (span[i] == Path.AltDirectorySeparatorChar)
			{
				span[i] = Path.DirectorySeparatorChar;
			}
		}
	}
}

sealed class CharMemComparator : IEqualityComparer<ReadOnlyMemory<char>>
{
	public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
	{
		return x.Span.SequenceEqual(y.Span);
	}

	public int GetHashCode(ReadOnlyMemory<char> x) => Utils.JavaStringHashcode(x.Span);
}
