using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CodingFS.Workspaces;
using System.Buffers;

namespace CodingFS;

public delegate Workspace? WorkspaceFactory(string path);

/// <summary>
/// 文件分类器，以给定的目录为上下文，对这之下的文件进行分类。
/// 
/// 【线程安全】
/// 该类不是线程安全的，请勿并发调用。
/// </summary>
public sealed class RootFileClassifier
{
	public int OuterDepth { get; set; } = int.MaxValue;

	public int InnerDepth { get; set; } = int.MaxValue;

	public string Root { get; }

	private readonly PathTrieNode<Workspace[]> rootNode;
	private readonly WorkspaceFactory[] factories;

	public RootFileClassifier(string root, Workspace[] globals, WorkspaceFactory[] factories)
	{
		Root = root;
		this.factories = factories;
		rootNode = new PathTrieNode<Workspace[]>(globals);
	}

	public IEnumerable<Workspace> GetWorkspaces(string file)
	{
		// 未检查是否属于该分类器的目录下
		var relative = Path.GetRelativePath(Root, file);
		var parts = relative.Split(Path.DirectorySeparatorChar);

		IEnumerable<Workspace> result = rootNode.Value;
		var node = rootNode;

		for (int i = 0; i < parts.Length; i++)
		{
			var part = parts[i];

			if (node.TryGetChild(part, out var child))
			{
				node = child;
			}
			else
			{
				var tempDir = Path.Join(Root, string.Join('\\', parts.Take(i + 1)));
				var matches = factories
					.Select(f => f(tempDir))
					.Where(x => x != null)!
					.ToArray<Workspace>();

				node = node.PutChild(part, matches);
			}

			result = result.Concat(node.Value);
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

	public IEnumerable<FileSystemInfo> ListFiles(string path, FileType type)
	{
		var @fixed = FixedOn(path);

		return new DirectoryInfo(path)
			.EnumerateFileSystemInfos()
			.Where(info => (@fixed.GetFileType(info.FullName) & type) != 0);
	}
}
