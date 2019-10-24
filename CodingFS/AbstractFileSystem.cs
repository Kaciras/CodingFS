﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using DokanNet;

namespace CodingFS
{
	/// <summary>
	/// IDokanOperations 的方法太多，而一般情况下并不需要用到全部，故该类提供它们的默认实现。
	/// 该类默认实现的文件系统只有一个空磁盘，里面没有任何文件，所有修改操作将成功返回但不产生任何效果。
	/// </summary>
	public abstract class AbstractFileSystem : IDokanOperations
	{
		public virtual NtStatus GetVolumeInformation(
			out string volumeLabel,
			out FileSystemFeatures features,
			out string fileSystemName,
			out uint maximumComponentLength,
			IDokanFileInfo info)
		{
			volumeLabel = "Dokan Virtual Drive";
			features = FileSystemFeatures.None;
			fileSystemName = "Dokan Virtual FS";
			maximumComponentLength = 256;
			return DokanResult.Success;
		}

		public virtual NtStatus ReadFile(
			string fileName,
			byte[] buffer, 
			out int bytesRead,
			long offset,
			IDokanFileInfo info)
		{
			bytesRead = 0;
			return DokanResult.Success;
		}

		public virtual NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
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
			return DokanResult.Success;
		}

		public virtual NtStatus FindFilesWithPattern(
			string fileName, 
			string searchPattern, 
			out IList<FileInformation> files, 
			IDokanFileInfo info)
		{
			files = Array.Empty<FileInformation>();
			return DokanResult.Success;
		}

		public virtual NtStatus FindFiles(string fileName, out IList<FileInformation>? files, IDokanFileInfo info)
		{
			return FindFilesWithPattern(fileName, "*", out files, info);
		}

		public virtual NtStatus FindStreams(string fileName, out IList<FileInformation>? streams, IDokanFileInfo info)
		{
			return FindFilesWithPattern(fileName, "*", out streams, info);
		}

		public virtual void Cleanup(string fileName, IDokanFileInfo info) {}

		public virtual void CloseFile(string fileName, IDokanFileInfo info) {}

		public virtual NtStatus CreateFile(
			string fileName, 
			DokanNet.FileAccess access, 
			FileShare share, 
			FileMode mode,
			FileOptions options,
			FileAttributes attributes,
			IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public virtual NtStatus GetDiskFreeSpace(
			out long freeBytesAvailable,
			out long totalNumberOfBytes,
			out long totalNumberOfFreeBytes,
			IDokanFileInfo info)
		{
			freeBytesAvailable = 1024 * 1024 * 1024 * 1024L;
			totalNumberOfBytes = 1024 * 1024 * 1024 * 1024L;
			totalNumberOfFreeBytes = 0;
			return NtStatus.Success;
		}

		public virtual NtStatus GetFileSecurity(
			string fileName,
			out FileSystemSecurity? security,
			AccessControlSections sections,
			IDokanFileInfo info)
		{
			security = null;
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public virtual NtStatus Mounted(IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public virtual NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public virtual NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public virtual NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public virtual NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public virtual NtStatus SetFileSecurity(
			string fileName,
			FileSystemSecurity security,
			AccessControlSections sections,
			IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public virtual NtStatus SetFileTime(
			string fileName,
			DateTime? creationTime,
			DateTime? lastAccessTime,
			DateTime? lastWriteTime,
			IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public virtual NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public virtual NtStatus Unmounted(IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public virtual NtStatus WriteFile(
			string fileName, 
			byte[] buffer, 
			out int bytesWritten,
			long offset, 
			IDokanFileInfo info)
		{
			bytesWritten = 0;
			return DokanResult.Success;
		}

		public virtual NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public virtual NtStatus DeleteFile(string fileName, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public virtual NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
		{
			return NtStatus.Success;
		}
	}
}
