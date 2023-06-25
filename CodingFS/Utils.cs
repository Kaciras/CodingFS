using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CodingFS;

internal static class Utils
{
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
