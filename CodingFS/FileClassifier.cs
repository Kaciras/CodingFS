using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CodingFS.Workspaces;

namespace CodingFS;

// TODO: 改成更省内存的数据结构
file struct TrieNode<T>
{
	Dictionary<ReadOnlyMemory<char>, TrieNode<T>>? children;

	public T Value { get; set; }

	public TrieNode(T value)
	{
		Value = value;
	}

	public readonly bool TryGet(
		ReadOnlyMemory<char> part,
		[MaybeNullWhen(false)] out TrieNode<T> child)
	{
		if (children == null)
		{
			child = default!;
			return false;
		}
		return children.TryGetValue(part, out child!);
	}

	public readonly void Remove(ReadOnlyMemory<char> part)
	{
		children?.Remove(part);
	}

	public TrieNode<T> Put(ReadOnlyMemory<char> part, T value)
	{
		children ??= new(Utils.memComparator);
		return children[part] = new TrieNode<T>(value);
	}
}

/// <summary>
/// 文件分类器，以给定的目录为上下文，对这之下的文件进行分类。
/// 
/// 【线程安全】
/// 该类不是线程安全的，请勿并发调用。
/// </summary>
public sealed class FileClassifier
{
	/// <summary>
	/// Maximum supported components length in file path.
	/// </summary>
	public const int MAX_COMPONENT = 255;

	public static readonly WorkspaceFactory[] FACTORIES =
	{
		new JetBrainsDetector().Detect,
		NpmWorkspace.Match,
		GitWorkspace.Match,
		MavenWorkspace.Match,
		CargoWorkspace.Match,
		VSCodeWorkspace.Match,
		VisualStudioWorkspace.Match,
	};

	public static readonly Workspace[] GLOBALS =
	{
		new CustomWorkspace(),
		new CommonWorkspace(),
	};

	// ===============================================================

	public int OuterDepth { get; set; } = int.MaxValue;

	public int InnerDepth { get; set; } = int.MaxValue;

	public string Root { get; }

	private readonly WorkspaceFactory[] factories;
	private readonly TrieNode<Workspace[]> cacheRoot;

	public FileClassifier(string root): this(root, FACTORIES, GLOBALS) {}

	public FileClassifier(string root, WorkspaceFactory[] factories, Workspace[] globals)
	{
		Root = root;
		this.factories = factories;
		cacheRoot = new TrieNode<Workspace[]>(globals);
	}

	static ReadOnlyMemory<char> NextPart(ReadOnlyMemory<char> path, int offset)
	{
		if (offset >= path.Length)
		{
			return ReadOnlyMemory<char>.Empty;
		}
		var i = path.Span[offset..].IndexOfAny('/', '\\');
		return path[offset..(i == -1 ? Index.End : offset + i)];
	}

	public void Invalid(string directory)
	{
		var memory = directory.AsMemory();
		var part = NextPart(memory, 0);
		var next = NextPart(memory, part.Length);

		var node = cacheRoot;
		while (!next.IsEmpty)
		{
			if (node.TryGet(part, out var child))
			{
				node = child;
			}
			else
			{
				return;
			}
			part = next;
			next = NextPart(memory, part.Length);
		}

		node.Remove(part);
	}

	public WorkspacesInfo GetWorkspaces(string directory)
	{
		var splitor = new PathComponentSpliter(directory);
		splitor.Relative(Root);
		splitor.NormalizeSepUnsafe();

		var node = cacheRoot;
		var workspaces = new List<Workspace>(node.Value);
		
		while (splitor.HasNext)
		{
			var part = splitor.SplitNext();
			if (node.TryGet(part, out var child))
			{
				node = child;
			}
			else
			{
				var tempDir = new string(splitor.Left.Span);
				var matches = new List<Workspace>();
				var ctx = new DetectContxt(workspaces, tempDir, matches);

				foreach (var factory in factories)
				{
					factory(ctx);
				}

				node = node.Put(part, matches.ToArray());
			}

			workspaces.AddRange(node.Value);
		}

		return new WorkspacesInfo(directory, workspaces, node.Value); 
	}
	
	public IEnumerable<(string, FileType)> Walk(string root, FileType includes)
	{
		// EnumerateFiles 和 EnumerateDirectories 都是在这基础上过滤的。
		foreach (var file in Directory.EnumerateFileSystemEntries(root))
		{
			var folder = Path.GetDirectoryName(file)!;
			var type = GetWorkspaces(folder).GetFileType(file);

			if (!includes.HasFlag(type))
			{
				continue;
			}
			yield return (file, type);

			if (Directory.Exists(file))
			{
				foreach (var i in Walk(file, includes)) yield return i;
			}
		}
	}
}
