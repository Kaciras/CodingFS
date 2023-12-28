using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using DokanNet;

[assembly: InternalsVisibleTo("CodingFS.Test")]
[assembly: InternalsVisibleTo("CodingFS.Benchmark")]
[module: SkipLocalsInit]

namespace CodingFS.Helper;

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

    public static int IndexOfNth(this string s, char c, int n)
    {
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == c && (--n) == 0)
                return i;
        }
        return -1;
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

    public static int IndexOfSpan(IReadOnlyList<string> list, ReadOnlySpan<char> value)
    {
        var length = list.Count;
        for (var i = 0; i < length; i++)
        {
            if (value.SequenceEqual(list[i])) return i;
        }
        return -1;
    }

    public static unsafe int JavaStringHashCode(ReadOnlySpan<char> str)
    {
        fixed (char* r = str)
        {
            var p = r;
            var e = p + str.Length;
            var h = 0;

            // C# does not optimize 31 * h to (h << 5) - h.
            while (p < e)
            {
                h = (h << 5) - h + *p++;
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
}

sealed class CharMemComparator : IEqualityComparer<ReadOnlyMemory<char>>
{
    public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
    {
        return x.Span.SequenceEqual(y.Span);
    }

    public int GetHashCode(ReadOnlyMemory<char> x) => string.GetHashCode(x.Span);
}
