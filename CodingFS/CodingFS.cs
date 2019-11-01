using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Linq;
using DokanNet;
using System.Diagnostics.CodeAnalysis;

namespace CodingFS
{
	public sealed class CodingFS : AbstractFileSystem
	{
		readonly string[] roots;
		readonly CodingFileScanner scanner;

		public CodingFS(params string[] roots)
		{
			this.roots = roots;
			scanner = new CodingFileScanner(roots);
		}
		
		private string MapPath(string value)
		{
			var fileRoot = value.Split(Path.DirectorySeparatorChar)[1];

			foreach (var root in roots)
			{
				if (fileRoot == Path.GetFileName(root))
				{
					return Path.Join(root, value.Substring(fileRoot.Length + 1));
				}
			}

			// 【问题】抛出异常后 out 参数是怎样的？
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
				files = roots.Select(path => MapInfo(new DirectoryInfo(path))).ToList();
			}
			else
			{
				files = new DirectoryInfo(MapPath(fileName))
					.EnumerateFileSystemInfos()
					.Where(file => scanner.GetFileType(file.FullName) == FileType.Source)
					.Select(MapInfo).ToList();
			}
			return DokanResult.Success;
		}

		public override NtStatus GetFileInformation(
			string fileName, 
			out FileInformation fileInfo, 
			IDokanFileInfo info)
		{
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
				var rawPath = MapPath(fileName);

				using var stream = new FileStream(rawPath, FileMode.Open);
				stream.Position = offset;
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
			volumeLabel = "CodingFS";
			features = FileSystemFeatures.None;
			fileSystemName = "CodingFS";
			maximumComponentLength = 256;
			return DokanResult.Success;
		}
	}
}
