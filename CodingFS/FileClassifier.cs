using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CodingFS.Workspaces;

namespace CodingFS
{
	public sealed class FileClassifier
	{
		// Directory 和 Path 都跟IO库里的镜头类重名了，只能用这个烂名字
		public string FullName { get; }

		private readonly PathTrieNode<IWorkspace[]> root;
		private readonly IWorkspaceFactory[] factories;

		public FileClassifier(string directory, IWorkspace[] globals, IWorkspaceFactory[] factories)
		{
			FullName = directory;
			root = new PathTrieNode<IWorkspace[]>(globals);
			this.factories = factories;
		}

		private IEnumerable<IWorkspace> GetWorkspaces(string file)
		{
			// 未检查是否属于该分类器的目录下
			var relative = Path.GetRelativePath(FullName, file);
			var parts = relative.Split(Path.DirectorySeparatorChar);

			var result = root.Value;
			var node = root;

			for (int i = 0; i < parts.Length; i++)
			{
				var part = parts[i];

				if (node.TryGetChild(part, out var child))
				{
					node = child;
				}

				var tempDir = Path.Join(FullName, string.Join('\\', parts.Take(i)));
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

		// 【分类依据】
		// 根据 IDE 和 VCS 找出被忽略的文件，未被忽略的都是和源文件，再由项目结构的约定
		// 从被忽略的文件里区分出依赖，最后剩下的都是生成的文件。
		private static FileType GetFileType(IEnumerable<IWorkspace> workspaces, string path)
		{
			var flags = workspaces.Aggregate(RecognizeType.NotCare, (v, c) => v | c.Recognize(path));
			if (flags.HasFlag(RecognizeType.Dependency))
			{
				return FileType.Dependency;
			}
			else if (flags.HasFlag(RecognizeType.Ignored))
			{
				return FileType.Build;
			}
			else
			{
				return FileType.Source;
			}
		}

		public FileType GetFileType(string path)
		{
			var directory = Path.GetDirectoryName(path);
			if (directory == null)
			{
				return GetFileType(root.Value, path);
			}
			return GetFileType(GetWorkspaces(directory), path);
		}

		public IEnumerable<(string, FileType)> EnumerateFiles(string directory)
		{
			var workspaces = GetWorkspaces(directory);
			return Directory.EnumerateFileSystemEntries(directory)
				.Select(file => (file, GetFileType(workspaces, file)));
		}
	}
}
