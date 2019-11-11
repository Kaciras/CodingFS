using System;
using System.IO;

namespace CodingFS.Workspaces
{
	public class RecognizedFileMap : IWorkspace
	{
		private readonly string directory;
		private readonly PathTrieNode<RecognizeType> root;

		public RecognizedFileMap(string directory)
		{
			this.directory = directory;
			root = new PathTrieNode<RecognizeType>(default);
		}

		public RecognizeType Recognize(string path)
		{
			path = EnsureRelativePath(path);

			var node = root;
			var parts = path.Split('\\', '/');

			foreach (var part in parts)
			{
				if (node.TryGetChild(part, out var child))
				{
					node = child;
				}
				else
				{
					return RecognizeType.NotCare;
				}
			}

			// 只有目录下才会有文件不确定，文件直接返回NotCare即可
			if (node.Value != RecognizeType.Uncertain)
			{
				return node.Value;
			}
			return Directory.Exists(Path.Join(directory, path)) 
				? RecognizeType.Uncertain : RecognizeType.NotCare;
		}

		public void Add(string path, RecognizeType type)
		{
			var parts = EnsureRelativePath(path).Split('\\', '/');
			var node = root;

			foreach (var part in parts)
			{
				if (node.TryGetChild(part, out var child))
				{
					node = child;
				}
				else
				{
					node = node.PutChild(part, RecognizeType.Uncertain);
				}
			}

			node.Value = type;
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
