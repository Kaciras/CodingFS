using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("CodingFS.Test")]
[assembly: InternalsVisibleTo("CodingFS.Benchmark")]
[module: SkipLocalsInit]

namespace CodingFS;

sealed class MemComparator : IEqualityComparer<ReadOnlyMemory<char>>
{
	public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
	{
		return x.Span.SequenceEqual(y.Span);
	}

	public int GetHashCode([DisallowNull] ReadOnlyMemory<char> obj)
	{
		return Utils.JavaStringHashcode(obj.Span);
	}
}

internal static class Utils
{
	public static readonly MemComparator memComparator = new();

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
