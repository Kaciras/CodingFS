using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using DokanNet;

namespace CodingFS.FUSE;

/// <summary>
/// Attempt to convert IO exception thrown in IDokanOperations to NtStatus,
/// or re-throw it if cannot convert.
/// </summary>
public sealed class ExceptionWrapper : IDokanOperations
{
	public IDokanOperations Native { get; }

	public ExceptionWrapper(IDokanOperations native)
	{
		Native = native;
	}
	
	internal static NtStatus HandleException(Exception e)
	{
		switch (e)
		{
			case DirectoryNotFoundException:
				return DokanResult.PathNotFound;
			case FileNotFoundException:
				return DokanResult.FileNotFound;
			case UnauthorizedAccessException:
				return DokanResult.AccessDenied;
		}
		return (uint)Marshal.GetHRForException(e) == 0x80070020 
			? DokanResult.SharingViolation : throw e;
	}

	#region ================== Delegated Methods ==================

	public NtStatus CreateFile(
		string fileName,
		DokanNet.FileAccess access,
		FileShare share,
		FileMode mode,
		FileOptions options,
		FileAttributes attributes,
		IDokanFileInfo info)
	{
		try
		{
			return Native.CreateFile(fileName, access,
				share, mode, options, attributes, info);
		}
		catch (Exception e)
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

	public NtStatus ReadFile(
		string fileName,
		byte[] buffer,
		out int bytesRead,
		long offset,
		IDokanFileInfo info)
	{
		try
		{
			return Native.ReadFile(fileName, buffer, out bytesRead, offset, info);
		}
		catch (Exception e)
		{
			bytesRead = default;
			return HandleException(e);
		}
	}

	public NtStatus WriteFile(
		string fileName,
		byte[] buffer,
		out int bytesWritten,
		long offset,
		IDokanFileInfo info)
	{
		try
		{
			return Native.WriteFile(fileName, buffer, out bytesWritten, offset, info);
		}
		catch (Exception e)
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
		catch (Exception e)
		{
			return HandleException(e);
		}
	}

	public NtStatus GetFileInformation(
		string fileName,
		out FileInformation fileInfo,
		IDokanFileInfo info)
	{
		try
		{
			return Native.GetFileInformation(fileName, out fileInfo, info);
		}
		catch (Exception e)
		{
			fileInfo = default;
			return HandleException(e);
		}
	}

	public NtStatus FindFiles(
		string fileName,
		out IList<FileInformation> files,
		IDokanFileInfo info)
	{
		try
		{
			return Native.FindFiles(fileName, out files, info);
		}
		catch (Exception e)
		{
			files = Array.Empty<FileInformation>();
			return HandleException(e);
		}
	}

	public NtStatus FindFilesWithPattern(
		string fileName,
		string searchPattern,
		out IList<FileInformation> files,
		IDokanFileInfo info)
	{
		try
		{
			return Native.FindFilesWithPattern(fileName, searchPattern, out files, info);
		}
		catch (Exception e)
		{
			files = Array.Empty<FileInformation>();
			return HandleException(e);
		}
	}

	public NtStatus SetFileAttributes(
		string fileName,
		FileAttributes attributes,
		IDokanFileInfo info)
	{
		try
		{
			return Native.SetFileAttributes(fileName, attributes, info);
		}
		catch (Exception e)
		{
			return HandleException(e);
		}
	}

	public NtStatus SetFileTime(
		string fileName,
		DateTime? creationTime,
		DateTime? lastAccessTime,
		DateTime? lastWriteTime,
		IDokanFileInfo info)
	{
		try
		{
			return Native.SetFileTime(fileName, creationTime,
				lastAccessTime, lastWriteTime, info);
		}
		catch (Exception e)
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
		catch (Exception e)
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
		catch (Exception e)
		{
			return HandleException(e);
		}
	}

	public NtStatus MoveFile(
		string oldName,
		string newName,
		bool replace,
		IDokanFileInfo info)
	{
		try
		{
			return Native.MoveFile(oldName, newName, replace, info);
		}
		catch (Exception e)
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
		catch (Exception e)
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
		catch (Exception e)
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
		catch (Exception e)
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
		catch (Exception e)
		{
			return HandleException(e);
		}
	}

	public NtStatus GetDiskFreeSpace(
		out long freeBytesAvailable,
		out long totalNumberOfBytes,
		out long totalNumberOfFreeBytes,
		IDokanFileInfo info)
	{
		try
		{
			return Native.GetDiskFreeSpace(
				out freeBytesAvailable,
				out totalNumberOfBytes,
				out totalNumberOfFreeBytes,
				info);
		}
		catch (Exception e)
		{
			freeBytesAvailable = default;
			totalNumberOfBytes = default;
			totalNumberOfFreeBytes = default;
			return HandleException(e);
		}
	}

	public NtStatus GetVolumeInformation(
		out string volumeLabel,
		out FileSystemFeatures features,
		out string fileSystemName,
		out uint maximumComponentLength,
		IDokanFileInfo info)
	{
		try
		{
			return Native.GetVolumeInformation(
				out volumeLabel,
				out features,
				out fileSystemName,
				out maximumComponentLength,
				info);
		}
		catch (Exception e)
		{
			volumeLabel = string.Empty;
			features = default;
			fileSystemName = string.Empty;
			maximumComponentLength = default;
			return HandleException(e);
		}
	}

	public NtStatus GetFileSecurity(
		string fileName,
		out FileSystemSecurity? security,
		AccessControlSections sections,
		IDokanFileInfo info)
	{
		try
		{
			return Native.GetFileSecurity(fileName, out security, sections, info);
		}
		catch (Exception e)
		{
			security = default;
			return HandleException(e);
		}
	}

	public NtStatus SetFileSecurity(
		string fileName,
		FileSystemSecurity security,
		AccessControlSections sections,
		IDokanFileInfo info)
	{
		try
		{
			return Native.SetFileSecurity(fileName, security, sections, info);
		}
		catch (Exception e)
		{
			return HandleException(e);
		}
	}

	public NtStatus Mounted(string mountPoint, IDokanFileInfo info)
	{
		try
		{
			return Native.Mounted(mountPoint, info);
		}
		catch (Exception e)
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
		catch (Exception e)
		{
			return HandleException(e);
		}
	}

	public NtStatus FindStreams(
		string fileName,
		out IList<FileInformation> streams,
		IDokanFileInfo info)
	{
		try
		{
			return Native.FindStreams(fileName, out streams, info);
		}
		catch (Exception e)
		{
			streams = Array.Empty<FileInformation>();
			return HandleException(e);
		}
	}

	#endregion
}
