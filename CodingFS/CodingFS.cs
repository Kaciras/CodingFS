using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DokanNet;

namespace CodingFS
{
	public sealed class CodingFS : AbstractFileSystem
	{
		private readonly FileType type;
		private readonly Dictionary<string, CodingFileScanner> scanners;

		public CodingFS(FileType type, params string[] roots)
		{
			this.type = type;
			scanners = new Dictionary<string, CodingFileScanner>();

			foreach (var root in roots)
			{
				scanners[Path.GetFileName(root)] = new CodingFileScanner(root);
			}
		}

		private string MapPath(string value)
		{
			var split = value.Split(Path.DirectorySeparatorChar, 3);

			if (scanners.TryGetValue(split[1], out var scanner))
			{
				if (split.Length < 3)
				{
					return scanner.FullName;
				}
				return Path.Join(scanner.FullName, split[2]);
			}
			throw new FileNotFoundException("文件不在映射区", value);
		}

		private FileInformation MapInfo(FileSystemInfo src) => new FileInformation
		{
			Attributes = src.Attributes,
			FileName = src.Name,
			LastAccessTime = src.LastAccessTime,
			CreationTime = src.CreationTime,
			LastWriteTime = src.LastWriteTime,
			Length = (src as FileInfo)?.Length ?? 0
		};

		public override NtStatus FindFilesWithPattern(
			string fileName,
			string searchPattern,
			out IList<FileInformation> files,
			IDokanFileInfo info)
		{
			if (fileName == @"\")
			{
				files = scanners.Values
					.Select(sc => MapInfo(new DirectoryInfo(sc.FullName)))
					.ToList();
			}
			else
			{
				var root = fileName.Split(Path.DirectorySeparatorChar, 3)[1];

				if (!scanners.TryGetValue(root, out var scanner))
				{
					throw new FileNotFoundException("文件不在映射区", root);
				}

				files = new DirectoryInfo(MapPath(fileName))
						.EnumerateFileSystemInfos()
						.Where(file => scanner.GetFileType(file.FullName) == type)
						.Select(MapInfo).ToList();
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
				fileInfo = MapInfo(rawInfo.Exists ? rawInfo : new DirectoryInfo(rawPath));
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
				using var stream = new FileStream(MapPath(fileName), FileMode.Open, System.IO.FileAccess.Read)
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
	}
}
