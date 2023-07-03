using System.Collections.Generic;
using System.IO;
using System.Linq;
using DokanNet;

namespace CodingFS.FUSE;

public class FilteredDokan : UnsafeRedirectDokan
{
	public Dictionary<string, CodingPathFilter> Map { get; } = new();

	public FileType Type { get; set; }

	public string Name { get; }

	/// <summary>
	/// Create a new CodingFS with name, the name will displayed as volume label.
	/// </summary>
	/// <param name="name"></param>
	public FilteredDokan(string name) { Name = name; }

	protected override string GetPath(string value)
	{
		var split = value.Split(Path.DirectorySeparatorChar, 3);

		if (Map.TryGetValue(split[1], out var scanner))
		{
			if (split.Length < 3)
			{
				return scanner.Root;
			}
			return Path.Join(scanner.Root, split[2]);
		}
		throw new FileNotFoundException("文件不在映射区", value);
	}

	public override NtStatus GetVolumeInformation(
		out string volumeLabel,
		out FileSystemFeatures features,
		out string fileSystemName,
		out uint maximumComponentLength,
		IDokanFileInfo info)
	{
		volumeLabel = Name;
		features = FileSystemFeatures.UnicodeOnDisk
			| FileSystemFeatures.CaseSensitiveSearch
			| FileSystemFeatures.PersistentAcls
			| FileSystemFeatures.SupportsRemoteStorage
			| FileSystemFeatures.CasePreservedNames;
		fileSystemName = "FilteredFS";
		maximumComponentLength = 256;
		return DokanResult.Success;
	}

	public override NtStatus CreateFile(string fileName, DokanNet.FileAccess access, 
		FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
	{
		if (fileName == @"\")
		{
			return DokanResult.Success;
		}
		return base.CreateFile(fileName, access, share, mode, options, attributes, info);
	}

	// 【知识点】IEnumerable.ToArray 直接操作数组而不是 .ToList().ToArray()，所以比 ToList() 更快
	// https://github.com/dotnet/corefx/blob/master/src/Common/src/System/Collections/Generic/EnumerableHelpers.cs

	public override NtStatus FindFiles(
		string fileName,
		out IList<FileInformation> files,
		IDokanFileInfo info)
	{
		if (fileName == @"\")
		{
			files = Map.Values
				.Select(sc => MapInfo(new DirectoryInfo(sc.Root)))
				.ToArray();
		}
		else
		{
			var root = fileName.Split(Path.DirectorySeparatorChar, 3)[1];

			if (!Map.TryGetValue(root, out var scanner))
			{
				throw new FileNotFoundException("文件不在映射区", root);
			}

			fileName = GetPath(fileName);
			var @fixed = scanner.GetWorkspaces(fileName);

			files = new DirectoryInfo(fileName)
				.EnumerateFileSystemInfos()
				.Where(info => @fixed.GetFileType(info.FullName) == Type)
				.Select(MapInfo).ToArray();
		}

		return DokanResult.Success;
	}
}
