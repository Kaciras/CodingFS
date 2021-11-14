using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DokanNet;
using AccessType = System.IO.FileAccess;

namespace CodingFS.VFS;

public class CodingFS : DokanOperationBase
{
	private readonly FileType type;
	private readonly Dictionary<string, RootFileClassifier> map;

	public CodingFS(FileType type, Dictionary<string, RootFileClassifier> map)
	{
		this.type = type;
		this.map = map;
	}

	public override NtStatus GetVolumeInformation(
		out string volumeLabel,
		out FileSystemFeatures features,
		out string fileSystemName,
		out uint maximumComponentLength,
		IDokanFileInfo info)
	{
		volumeLabel = $"CodingFS({type})";
		features = FileSystemFeatures.None;
		fileSystemName = "CodingFS";
		maximumComponentLength = 256;
		return DokanResult.Success;
	}

	protected string MapPath(string value)
	{
		var split = value.Split(Path.DirectorySeparatorChar, 3);

		if (map.TryGetValue(split[1], out var scanner))
		{
			if (split.Length < 3)
			{
				return scanner.Root;
			}
			return Path.Join(scanner.Root, split[2]);
		}
		throw new FileNotFoundException("文件不在映射区", value);
	}

	// 【知识点】IEnumerable.ToArray 直接操作数组而不是 .ToList().ToArray()，所以比 ToList() 更快
	// https://github.com/dotnet/corefx/blob/master/src/Common/src/System/Collections/Generic/EnumerableHelpers.cs

	public override NtStatus FindFilesWithPattern(
		string fileName,
		string searchPattern,
		out IList<FileInformation> files,
		IDokanFileInfo info)
	{
		if (fileName == @"\")
		{
			files = map.Values
				.Select(sc => Conversion.MapInfo(new DirectoryInfo(sc.Root)))
				.ToArray();
		}
		else
		{
			var root = fileName.Split(Path.DirectorySeparatorChar, 3)[1];

			if (!map.TryGetValue(root, out var scanner))
			{
				throw new FileNotFoundException("文件不在映射区", root);
			}

			fileName = MapPath(fileName);
			var @fixed = scanner.FixedOn(fileName);

			files = new DirectoryInfo(fileName)
				.EnumerateFileSystemInfos()
				.Where(info => @fixed.GetFileType(info.FullName) == type)
				.Select(Conversion.MapInfo).ToArray();
		}

		return DokanResult.Success;
	}

	public override NtStatus GetFileInformation(
		string fileName,
		out FileInformation fileInfo,
		IDokanFileInfo info)
	{
		// 【坑】这个根文件夹必须要有，否则会出现许多奇怪的错误
		if (fileName == @"\")
		{
			fileInfo = new FileInformation
			{
				FileName = fileName,
				Length = 0,
				Attributes = FileAttributes.Directory,
				CreationTime = DateTime.Now,
				LastAccessTime = DateTime.Now,
				LastWriteTime = DateTime.Now,
			};
		}
		else
		{
			// 哪个傻逼想出来的文件和目录分开的API？
			var rawPath = MapPath(fileName);
			FileSystemInfo rawInfo = new FileInfo(rawPath);
			fileInfo = Conversion.MapInfo(rawInfo.Exists ? rawInfo : new DirectoryInfo(rawPath));
		}
		return DokanResult.Success;
	}

	public override NtStatus ReadFile(
		string fileName,
		byte[] buffer,
		out int bytesRead,
		long offset,
		IDokanFileInfo info)
	{
		if (info.Context == null)
		{
			// FileAccess 默认是 ReadWrite，会造成额外的锁定
			using var stream = new FileStream(MapPath(fileName), FileMode.Open, AccessType.Read)
			{
				Position = offset,
			};
			bytesRead = stream.Read(buffer, 0, buffer.Length);
		}
		else
		{
			var stream = (FileStream)info.Context;

			// Protect from overlapped read
			lock (stream)
			{
				stream.Position = offset;
				bytesRead = stream.Read(buffer, 0, buffer.Length);
			}
		}
		return DokanResult.Success;
	}
}
