using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CodingFS.Workspaces;
using System.Buffers;

namespace CodingFS
{
	/// <summary>
	/// 文件分类器，以给定的目录为上下文，对这之下的文件进行分类。
	/// 
	/// 【线程安全】
	/// 该类不是线程安全的，请勿并发调用。
	/// </summary>
	public sealed class RootFileClassifier
	{
		// Directory 和 Path 都跟IO库里的镜头类重名了，只能用这个烂名字
		public string Root { get; }

		private readonly PathTrieNode<IWorkspace[]> rootNode;
		private readonly IWorkspaceFactory[] factories;

		public RootFileClassifier(string root, IWorkspace[] globals, IWorkspaceFactory[] factories)
		{
			Root = root;
			rootNode = new PathTrieNode<IWorkspace[]>(globals);
			this.factories = factories;
		}

		private IEnumerable<IWorkspace> GetWorkspaces(string file)
		{
			// 未检查是否属于该分类器的目录下
			var relative = Path.GetRelativePath(Root, file);
			var parts = relative.Split(Path.DirectorySeparatorChar);

			var result = rootNode.Value;
			var node = rootNode;

			for (int i = 0; i < parts.Length; i++)
			{
				var part = parts[i];

				if (node.TryGetChild(part, out var child))
				{
					node = child;
				}
				
				var tempDir = Path.Join(Root, string.Join('\\', parts.Take(i + 1)));
				var matches = factories
					.Select(f => f.Match(tempDir))
					.Where(x => x != null)!
					.ToArray<IWorkspace>();

				node = node.PutChild(part, matches);

				if (node.Value.Length > 0)
				{
					return result.Concat(node.Value);
				}
			}

			return result;
		}

		public WorkspaceFileClassifier FixedOn(string? directory)
		{
			if (directory == null)
			{
				return new WorkspaceFileClassifier(rootNode.Value);
			}
			return new WorkspaceFileClassifier(GetWorkspaces(directory));
		}

		public FileType GetFileType(string path)
		{
			var directory = Path.GetDirectoryName(path);
			return FixedOn(directory).GetFileType(path);
		}
	}
}
