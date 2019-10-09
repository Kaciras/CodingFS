using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DokanNet;

namespace CodingFS
{
	class CodingFS : IDokanOperations
	{
		public void Cleanup(string fileName, DokanFileInfo info){}

		public void CloseFile(string fileName, DokanFileInfo info){}

		public NtStatus CreateFile(
			string fileName,
			DokanNet.FileAccess access, 
			FileShare share, FileMode mode,
			FileOptions options, 
			FileAttributes attributes, 
			DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus DeleteDirectory(string fileName, DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus DeleteFile(string fileName, DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
		{
			files = new List<FileInformation>();
			var helloTxt = new FileInformation
			{
				FileName = "hello.txt",
				Attributes = FileAttributes.Normal,
				Length = 11,
				CreationTime = DateTime.Now,
				LastWriteTime = DateTime.Now,
				LastAccessTime = DateTime.Now,
			};
			files.Add(helloTxt);
			return NtStatus.Success;
		}

		public NtStatus FindFilesWithPattern(
			string fileName, 
			string searchPattern, 
			out IList<FileInformation> files,
			DokanFileInfo info)
		{
			return FindFiles(fileName, out files, info);
		}

		public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, DokanFileInfo info)
		{
			return FindFiles(fileName, out streams, info);
		}

		public NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
		{
			return NtStatus.Success;
		}

		public NtStatus GetDiskFreeSpace(
			out long freeBytesAvailable, 
			out long totalNumberOfBytes, 
			out long totalNumberOfFreeBytes,
			DokanFileInfo info)
		{
			freeBytesAvailable = 10 * 1048576;
			totalNumberOfBytes = 20 * 1048576;
			totalNumberOfFreeBytes = 10 * 1048576;
			return NtStatus.Success;
		}

		public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
		{
			fileInfo = new FileInformation { FileName = fileName };

			if (fileName == "\\")
			{
				fileInfo.Attributes = FileAttributes.Directory;
				fileInfo.CreationTime = null;
				fileInfo.LastAccessTime = DateTime.Now;
				fileInfo.LastWriteTime = null;
			}
			else
			{
				fileInfo.Attributes = FileAttributes.Normal;
				fileInfo.Length = 11;
				fileInfo.CreationTime = DateTime.Now;
				fileInfo.LastAccessTime = DateTime.Now;
				fileInfo.LastWriteTime = DateTime.Now;

			}
			return DokanResult.Success;
		}

		public NtStatus GetFileSecurity(
			string fileName,
			out FileSystemSecurity security,
			AccessControlSections sections,
			DokanFileInfo info)
		{
			security = null;
			return NtStatus.NotImplemented;
		}

		public NtStatus GetVolumeInformation(
			out string volumeLabel, 
			out FileSystemFeatures features, 
			out string fileSystemName,
			out uint maximumComponentLength,
			DokanFileInfo info)
		{
			volumeLabel = "CodingFS";
			features = FileSystemFeatures.None;
			fileSystemName = string.Empty;
			maximumComponentLength = 256;
			return DokanResult.Success;
		}

		public NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus Mounted(DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
		{
			return DokanResult.Success;
		}
		
		public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
		{
			var readCount = Math.Min(11, buffer.Length - (int)offset);
			Encoding.UTF8.GetBytes("hello world", (int)offset, readCount, buffer, 0);
			bytesRead = buffer.Length;
			return DokanResult.Success;
		}

		public NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus Unmounted(DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
		{
			bytesWritten = 0;
			return DokanResult.Success;
		}
	}
}
