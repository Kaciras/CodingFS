using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Linq;
using DokanNet;
using System.Diagnostics.CodeAnalysis;

namespace CodingFS
{
	public class CodingFS : IDokanOperations
	{
		readonly string[] roots;

		public CodingFS(params string[] roots)
		{
			this.roots = roots;
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

		public NtStatus FindFilesWithPattern(
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
				files = new DirectoryInfo(MapPath(fileName)).EnumerateFileSystemInfos().Select(MapInfo).ToList();
			}
			return DokanResult.Success;
		}

		public NtStatus GetFileInformation(
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

		public NtStatus ReadFile(
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

		public NtStatus GetVolumeInformation(
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

		#region ===================== 下面的不重要 =====================

		public NtStatus FindFiles(string fileName, out IList<FileInformation>? files, IDokanFileInfo info)
		{
			return FindFilesWithPattern(fileName, "*", out files, info);
		}

		public NtStatus FindStreams(string fileName, out IList<FileInformation>? streams, IDokanFileInfo info)
		{
			return FindFilesWithPattern(fileName, "*", out streams, info);
		}

		public void Cleanup(string fileName, IDokanFileInfo info)
		{
		}

		public void CloseFile(string fileName, IDokanFileInfo info)
		{
		}

		public NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus GetDiskFreeSpace(
			out long freeBytesAvailable,
			out long totalNumberOfBytes,
			out long totalNumberOfFreeBytes,
			IDokanFileInfo info)
		{
			freeBytesAvailable = 1048576;
			totalNumberOfBytes = 1048576;
			totalNumberOfFreeBytes = 0;
			return NtStatus.Success;
		}

		public NtStatus GetFileSecurity(
			string fileName,
			out FileSystemSecurity? security,
			AccessControlSections sections,
			IDokanFileInfo info)
		{
			security = null;
			return NtStatus.NotImplemented;
		}

		public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus Mounted(IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus SetFileSecurity(
			string fileName,
			FileSystemSecurity security,
			AccessControlSections sections,
			IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus SetFileTime(
			string fileName,
			DateTime? creationTime,
			DateTime? lastAccessTime,
			DateTime? lastWriteTime,
			IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus Unmounted(IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
		{
			bytesWritten = 0;
			return DokanResult.Success;
		}

		#endregion
	}
}
