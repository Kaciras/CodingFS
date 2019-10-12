using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using DokanNet;

namespace CodingFS
{
	public sealed class StaticFSProxy : IDokanOperations
	{
		public IDokanOperations Native { get; }

		public StaticFSProxy(IDokanOperations native)
		{
			Native = native;
		}

		/// <summary>
		/// 把一些IO异常转换为对应的NtStatus,如果不能转换则原样抛出。
		/// </summary>
		/// <param name="e">异常</param>
		/// <returns>对应的NtStatus</returns>
		internal static NtStatus HandleException(Exception e) => e switch
		{
			FileNotFoundException _ => DokanResult.FileNotFound,
			DirectoryNotFoundException _ => DokanResult.FileNotFound,
			_ => throw e,
		};

		#region ===================== 下面全是代理 =====================

		public NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
		{
			try
			{
				return Native.CreateFile(fileName, access, share, mode, options, attributes, info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public void Cleanup(string fileName, IDokanFileInfo info)
		{
			Native.Cleanup(fileName, info);
		}

		public void CloseFile(string fileName, IDokanFileInfo info)
		{
			Native.CloseFile(fileName, info);
		}

		public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
		{
			try
			{
				return Native.ReadFile(fileName, buffer, out bytesRead, offset, info);
			}
			catch (IOException e)
			{
				bytesRead = default;
				return HandleException(e);
			}
		}

		public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
		{
			try
			{
				return Native.WriteFile(fileName, buffer, out bytesWritten, offset, info);
			}
			catch (IOException e)
			{
				bytesWritten = default;
				return HandleException(e);
			}
		}

		public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
		{
			try
			{
				return Native.FlushFileBuffers(fileName, info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
		{
			try
			{
				return Native.GetFileInformation(fileName, out fileInfo, info);
			}
			catch (IOException e)
			{
				fileInfo = default;
				return HandleException(e);
			}
		}

		public NtStatus FindFiles(string fileName, out IList<FileInformation>? files, IDokanFileInfo info)
		{
			try
			{
				return Native.FindFiles(fileName, out files, info);
			}
			catch (IOException e)
			{
				files = default;
				return HandleException(e);
			}
		}

		public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation>? files, IDokanFileInfo info)
		{
			try
			{
				return Native.FindFilesWithPattern(fileName, searchPattern, out files, info);
			}
			catch (IOException e)
			{
				files = default;
				return HandleException(e);
			}
		}

		public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
		{
			try
			{
				return Native.SetFileAttributes(fileName, attributes, info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info)
		{
			try
			{
				return Native.SetFileTime(fileName, creationTime, lastAccessTime, lastWriteTime, info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
		{
			try
			{
				return Native.DeleteFile(fileName, info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
		{
			try
			{
				return Native.DeleteDirectory(fileName, info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
		{
			try
			{
				return Native.MoveFile(oldName, newName, replace, info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
		{
			try
			{
				return Native.SetEndOfFile(fileName, length, info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
		{
			try
			{
				return Native.SetAllocationSize(fileName, length, info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
		{
			try
			{
				return Native.LockFile(fileName, offset, length, info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
		{
			try
			{
				return Native.UnlockFile(fileName, offset, length, info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info)
		{
			try
			{
				return Native.GetDiskFreeSpace(out freeBytesAvailable, out totalNumberOfBytes, out totalNumberOfFreeBytes, info);
			}
			catch (IOException e)
			{
				freeBytesAvailable = default;
				totalNumberOfBytes = default;
				totalNumberOfFreeBytes = default;
				return HandleException(e);
			}
		}

		public NtStatus GetVolumeInformation(out string? volumeLabel, out FileSystemFeatures features, out string? fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
		{
			try
			{
				return Native.GetVolumeInformation(out volumeLabel, out features, out fileSystemName, out maximumComponentLength, info);
			}
			catch (IOException e)
			{
				volumeLabel = default;
				features = default;
				fileSystemName = default;
				maximumComponentLength = default;
				return HandleException(e);
			}
		}

		public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity? security, AccessControlSections sections, IDokanFileInfo info)
		{
			try
			{
				return Native.GetFileSecurity(fileName, out security, sections, info);
			}
			catch (IOException e)
			{
				security = default;
				return HandleException(e);
			}
		}

		public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
		{
			try
			{
				return Native.SetFileSecurity(fileName, security, sections, info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public NtStatus Mounted(IDokanFileInfo info)
		{
			try
			{
				return Native.Mounted(info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public NtStatus Unmounted(IDokanFileInfo info)
		{
			try
			{
				return Native.Unmounted(info);
			}
			catch (IOException e)
			{
				return HandleException(e);
			}
		}

		public NtStatus FindStreams(string fileName, out IList<FileInformation>? streams, IDokanFileInfo info)
		{
			try
			{
				return Native.FindStreams(fileName, out streams, info);
			}
			catch (IOException e)
			{
				streams = default;
				return HandleException(e);
			}
		}

		#endregion
	}
}
