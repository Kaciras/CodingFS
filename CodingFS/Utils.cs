using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DokanNet;

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

	public static bool IsFile(string p0, string p1)
	{
		return File.Exists(Path.Join(p0, p1));
	}

	public static bool IsFile(params string[] paths)
	{
		if (paths.Length == 1)
		{
			return File.Exists(paths[0]);
		}
		return File.Exists(Path.Join(paths));
	}

	public static bool IsDir(string p0, string p1)
	{
		return Directory.Exists(Path.Join(p0, p1));
	}

	public static unsafe int JavaStringHashCode(ReadOnlySpan<char> str)
	{
		var h = 0;
		fixed (char* r = str)
		{
			var p = r;
			var e = p + str.Length;

			// C# does not optimize 31 * h to (h << 5) - h.
			while (p < e)
			{
				h = (h << 5) - h + *p;
				p += 1;
			}
			return h;
		}
	}

	/// <summary>
	/// Convert FileSystemInfo to Dokan's FileInformation.
	/// </summary>
	public static FileInformation ConvertFSInfo(FileSystemInfo src) => new()
	{
		Attributes = src.Attributes,
		FileName = src.Name,
		LastAccessTime = src.LastAccessTime,
		CreationTime = src.CreationTime,
		LastWriteTime = src.LastWriteTime,
		Length = src is FileInfo file ? file.Length : 0,
	};

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

	public int GetHashCode(ReadOnlyMemory<char> x) => string.GetHashCode(x.Span);
}
