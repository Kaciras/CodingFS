using System;
using System.Collections.Generic;
using System.IO;

namespace CodingFS.Workspaces
{
	public class PathDict : IWorkspace
	{
		private readonly string directory;
		private readonly IDictionary<FilePath, RecognizeType> map;

		public PathDict(string directory)
		{
			this.directory = directory;
			map = new Dictionary<FilePath, RecognizeType>();
		}

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
}
