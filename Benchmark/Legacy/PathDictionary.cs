using System;
using System.Collections.Generic;
using System.IO;

namespace CodingFS.Benchmark.Legacy;

public sealed class PathDictionary(string directory)
{
	readonly string directory = directory;
	readonly Dictionary<FilePath, RecognizeType> map = new();

	public RecognizeType Recognize(string path)
	{
		path = EnsureRelativePath(path);
		if (map.TryGetValue(path, out var result))
		{
			return result;
		}
		return RecognizeType.NotCare;
	}

	public void Add(string path, RecognizeType type)
	{
		map[EnsureRelativePath(path)] = type;
	}

	// 两个便捷方法，没有 AddNotCare 因为 NotCare 不用添加

	public void AddDependency(string path) => Add(path, RecognizeType.Dependency);

	public void AddIgnore(string path) => Add(path, RecognizeType.Ignored);

	/// <summary>
	/// 检查路径是相对路径，如果不是则尝试转换，转换失则败抛出ArgumentException。
	/// </summary>
	/// <param name="path">路径</param>
	/// <returns>本目录之下的相对路径</returns>
	private string EnsureRelativePath(string path)
	{
		if (!Path.IsPathRooted(path))
		{
			return path;
		}
		var relative = Path.GetRelativePath(directory, path);
		if (relative != path)
		{
			return relative;
		}
		throw new ArgumentException($"路径必须处于{directory}下");
	}
}