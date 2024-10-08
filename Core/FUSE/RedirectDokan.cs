/*
 * Copyright (C) 2015 - 2019 Adrien J. <liryna.stark@gmail.com> and Maxime C. <maxime@islog.com>
 * Copyright (c) 2007 Hiroki Asakawa
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using CodingFS.Helper;
using DokanNet;
using Microsoft.Win32.SafeHandles;
using AccessType = System.IO.FileAccess;
using FileAccess = DokanNet.FileAccess;

#pragma warning disable CA1416 // Dokan is only work on Windows.

namespace CodingFS.FUSE;

/// <summary>
/// Just forward operations to the real path.
/// <br/>
/// Derived from https://github.com/dokan-dev/dokan-dotnet/blob/master/sample/DokanNetMirror/Mirror.cs
/// </summary>
abstract partial class RedirectDokan : IDokanOperations
{
	const FileAccess DATA_ACCESS = FileAccess.Execute | FileAccess.GenericExecute
								| FileAccess.GenericWrite | FileAccess.GenericRead
								| FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData;

	const FileAccess WRITE_ACCESS = FileAccess.WriteData | FileAccess.AppendData
								| FileAccess.Delete | FileAccess.GenericWrite;

	// Operations that not allowed for directory.
	const FileAccess SYNC_FILEOPS = FileAccess.GenericRead | FileAccess.Delete;

	const long FREE_SPACE = 10 * 1024 * 1024 * 1024L;
	const int BUFFER_SIZE = 4096;

	protected string mountPoint = null!;

	protected abstract string GetPath(string fileName);

	protected static int NumOfBytesToCopy(int bufferLength, long offset, IDokanFileInfo info, FileStream stream)
	{
		if (info.PagingIo)
		{
			var longDistance = stream.Length - offset;
			if (longDistance > int.MaxValue)
			{
				return bufferLength;
			}
			var distance = (int)longDistance;
			if (distance < bufferLength) return distance;
		}
		return bufferLength;
	}

	#region Implementation of IDokanOperations

	static FileAttributes AttrsOrDefault(string path)
	{
		try
		{
			return File.GetAttributes(path);
		}
		catch (IOException)
		{
			// Does an existing file always have attributes?
			return FileAttributes.None;
		}
	}

	public virtual NtStatus CreateFile(
		string fileName, FileAccess access, FileShare share, FileMode mode,
		FileOptions options, FileAttributes newAttrs, IDokanFileInfo info)
	{
		fileName = GetPath(fileName);

		if (info.IsDirectory)
		{
			return CreateDirectory(fileName, mode);
		}

		var attrs = AttrsOrDefault(fileName);
		var exists = attrs != FileAttributes.None;
		switch (mode, exists)
		{
			case (FileMode.Open, false):
				return DokanResult.FileNotFound;

			case (FileMode.Open, true):
				var attributesOnly = (access & DATA_ACCESS) == 0;
				var isDir = (attrs & FileAttributes.Directory) != 0;

				// Only wants to read attrs, security info, or open directory.
				if (attributesOnly || isDir)
				{
					if (isDir &&
						(access & FileAccess.Synchronize) == 0 &&
						(access & SYNC_FILEOPS) != 0)
					{
						return DokanResult.AccessDenied;
					}

					// must set it to something if you return DokanError.Success
					info.Context = new object();
					info.IsDirectory = isDir;
					return DokanResult.Success;
				}
				break;

			case (FileMode.CreateNew, true):
				return DokanResult.FileExists;

			case (FileMode.Truncate, false):
				return DokanResult.FileNotFound;
		}

		try
		{
			var readAccess = (access & WRITE_ACCESS) == 0;
			var streamAccess = readAccess ? AccessType.Read : AccessType.ReadWrite;
			var result = DokanResult.Success;

			if (mode == FileMode.CreateNew && readAccess)
				streamAccess = AccessType.ReadWrite;

			info.Context = new FileStream(fileName, mode,
				streamAccess, share, BUFFER_SIZE, options);

			if (exists && (mode == FileMode.OpenOrCreate || mode == FileMode.Create))
				result = DokanResult.AlreadyExists;

			var fileCreated = mode == FileMode.CreateNew
				|| mode == FileMode.Create
				|| (!exists && mode == FileMode.OpenOrCreate);

			if (fileCreated)
			{
				newAttrs |= FileAttributes.Archive; // Files are always created as Archive
				newAttrs &= ~FileAttributes.Normal; // Normal is override if any other attribute is set.
				File.SetAttributes(fileName, newAttrs);
			}
			return result;
		}
		catch (UnauthorizedAccessException)
		{
			// returning AccessDenied cleanup and close won't be called,
			// so we have to take care of the stream now.
			CloseFile(fileName, info);
			return DokanResult.AccessDenied;
		}
	}

	static NtStatus CreateDirectory(string fileName, FileMode mode)
	{
		var attrs = AttrsOrDefault(fileName);
		switch (mode, attrs != FileAttributes.None)
		{
			case (FileMode.CreateNew, false):
				Directory.CreateDirectory(fileName);
				return DokanResult.Success;

			case (FileMode.CreateNew, true):
				return (attrs & FileAttributes.Directory) != 0
					? DokanResult.FileExists
					: DokanResult.AlreadyExists;

			case (FileMode.Open, false):
				return DokanResult.FileNotFound;

			case (FileMode.Open, true):
				if ((attrs & FileAttributes.Directory) == 0)
					return DokanResult.NotADirectory;

				// Check you can list the directory.
				_ = Directory.EnumerateFileSystemEntries(fileName).Any();
				return DokanResult.Success;

			default:
				return DokanResult.Success;
		}
	}

	public virtual void Cleanup(string fileName, IDokanFileInfo info)
	{
		CloseFile(fileName, info);

		if (info.DeleteOnClose)
		{
			if (info.IsDirectory)
			{
				Directory.Delete(GetPath(fileName));
			}
			else
			{
				File.Delete(GetPath(fileName));
			}
		}
	}

	public virtual void CloseFile(string fileName, IDokanFileInfo info)
	{
		if (info.Context is FileStream stream)
		{
			stream.Dispose();
			info.Context = null;
		}
		// Could recreate cleanup code here but this is not called sometimes
	}

	public virtual NtStatus ReadFile(
		string fileName,
		byte[] buffer,
		out int bytesRead,
		long offset,
		IDokanFileInfo info)
	{
		if (info.Context == null)
		{
			using var stream = new FileStream(GetPath(fileName), FileMode.Open, AccessType.Read);
			stream.Position = offset;
			bytesRead = stream.Read(buffer, 0, buffer.Length);
		}
		else
		{
			var stream = (FileStream)info.Context;
			lock (stream) // Protect from overlapped read
			{
				stream.Position = offset;
				bytesRead = stream.Read(buffer, 0, buffer.Length);
			}
		}
		return DokanResult.Success;
	}

	public virtual NtStatus WriteFile(
		string fileName,
		byte[] buffer,
		out int bytesWritten,
		long offset,
		IDokanFileInfo info)
	{
		// Offset of -1 is an APPEND: https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-writefile
		var append = offset == -1;

		if (info.Context == null)
		{
			var mode = append ? FileMode.Append : FileMode.Open;
			using var stream = new FileStream(GetPath(fileName), mode, AccessType.Write);
			if (!append)
			{
				stream.Position = offset;
			}
			bytesWritten = NumOfBytesToCopy(buffer.Length, offset, info, stream);
			stream.Write(buffer, 0, bytesWritten);
		}
		else
		{
			var stream = (FileStream)info.Context;
			lock (stream) // Protect from overlapped write
			{
				if (append)
				{
					if (stream.CanSeek)
					{
						stream.Seek(0, SeekOrigin.End);
					}
					else
					{
						bytesWritten = 0;
						return DokanResult.Error;
					}
				}
				else
				{
					stream.Position = offset;
				}
				bytesWritten = NumOfBytesToCopy(buffer.Length, offset, info, stream);
				stream.Write(buffer, 0, bytesWritten);
			}
		}
		return DokanResult.Success;
	}

	public virtual NtStatus FlushFileBuffers(
		string fileName,
		IDokanFileInfo info)
	{
		try
		{
			((FileStream)info.Context).Flush();
			return DokanResult.Success;
		}
		catch (IOException)
		{
			return DokanResult.DiskFull;
		}
	}

	public virtual NtStatus GetFileInformation(
		string fileName,
		out FileInformation fileInfo,
		IDokanFileInfo info)
	{
		// Attributes of the volume, which filename is \
		if (fileName == @"\")
		{
			fileInfo = new FileInformation
			{
				FileName = fileName,
				Length = 0,
				Attributes = FileAttributes.Directory,
			};
		}
		else
		{
			// 哪个傻逼想出来的文件和目录分开的 API？
			var realPath = GetPath(fileName);
			FileSystemInfo rawInfo = new FileInfo(realPath);

			if (rawInfo.Exists)
			{
				fileInfo = Utils.ConvertFSInfo(rawInfo);
			}
			else
			{
				rawInfo = new DirectoryInfo(realPath);
				if (rawInfo.Exists)
				{
					fileInfo = Utils.ConvertFSInfo(rawInfo);
				}
				else
				{
					fileInfo = default;
					return DokanResult.PathNotFound;
				}
			}
		}
		return DokanResult.Success;
	}

	public virtual NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
	{
		// This function is not called because FindFilesWithPattern is implemented
		// Return DokanResult.NotImplemented in FindFilesWithPattern to make FindFiles called
		files = FindFilesHelper(fileName, "*");
		return DokanResult.Success;
	}

	public virtual NtStatus FindStreams(
		string fileName,
		out IList<FileInformation> streams,
		IDokanFileInfo info)
	{
		streams = Array.Empty<FileInformation>();
		return DokanResult.NotImplemented;
	}

	public virtual IList<FileInformation> FindFilesHelper(
		string fileName,
		string searchPattern)
	{
		return new DirectoryInfo(GetPath(fileName))
			.EnumerateFileSystemInfos()
			.Where(finfo => DokanHelper.DokanIsNameInExpression(searchPattern, finfo.Name, true))
			.Select(Utils.ConvertFSInfo).ToArray();
	}

	public virtual NtStatus FindFilesWithPattern(
		string fileName,
		string searchPattern,
		out IList<FileInformation> files,
		IDokanFileInfo info)
	{
		files = Array.Empty<FileInformation>();
		return DokanResult.NotImplemented;
	}

	public virtual NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
	{
		// MS-FSCC 2.6 File Attributes : There is no file attribute with the value 0x00000000
		// because a value of 0x00000000 in the FileAttributes field means that the file newAttrs for this file MUST NOT be changed when setting basic information for the file
		if (attributes != 0)
			File.SetAttributes(GetPath(fileName), attributes);
		return DokanResult.Success;
	}

	public virtual NtStatus SetFileTime(
		string fileName,
		DateTime? creationTime,
		DateTime? lastAccessTime,
		DateTime? lastWriteTime,
		IDokanFileInfo info)
	{
		if (info.Context is FileStream stream)
		{
			var ct = creationTime?.ToFileTime() ?? 0;
			var lat = lastAccessTime?.ToFileTime() ?? 0;
			var lwt = lastWriteTime?.ToFileTime() ?? 0;

			if (SetFileTime(stream.SafeFileHandle, ref ct, ref lat, ref lwt))
				return DokanResult.Success;
			throw Marshal.GetExceptionForHR(Marshal.GetLastWin32Error())!;
		}

		fileName = GetPath(fileName);

		if (creationTime.HasValue)
			File.SetCreationTime(fileName, creationTime.Value);
		if (lastAccessTime.HasValue)
			File.SetLastAccessTime(fileName, lastAccessTime.Value);
		if (lastWriteTime.HasValue)
			File.SetLastWriteTime(fileName, lastWriteTime.Value);

		return DokanResult.Success;
	}

	public virtual NtStatus DeleteFile(string fileName, IDokanFileInfo info)
	{
		fileName = GetPath(fileName);
		try
		{
			var attrs = File.GetAttributes(fileName);
			if ((attrs & FileAttributes.Directory) != 0)
			{
				return DokanResult.AccessDenied;
			}
		}
		catch (IOException e)
		when (e is FileNotFoundException || e is DirectoryNotFoundException)
		{
			return DokanResult.FileNotFound;
		}

		return DokanResult.Success; // The true deletion is in Cleanup().
	}

	public virtual NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
	{
		// If dir is not empty it can't be deleted
		return Directory.EnumerateFileSystemEntries(GetPath(fileName)).Any()
				? DokanResult.DirectoryNotEmpty : DokanResult.Success;
	}

	public virtual NtStatus MoveFile(
		string oldName,
		string newName,
		bool replace,
		IDokanFileInfo info)
	{
		CloseFile(oldName, info);
		oldName = GetPath(oldName);
		newName = GetPath(newName);

		var exist = info.IsDirectory
			? Directory.Exists(newName)
			: File.Exists(newName);

		if (!exist)
		{
			info.Context = null;

			if (info.IsDirectory)
				Directory.Move(oldName, newName);
			else
				File.Move(oldName, newName);
			return DokanResult.Success;
		}
		else if (replace)
		{
			info.Context = null;

			// Cannot replace directory destination - See MOVEFILE_REPLACE_EXISTING
			if (info.IsDirectory)
				return DokanResult.AccessDenied;

			File.Move(oldName, newName, true);
			return DokanResult.Success;
		}

		return DokanResult.FileExists;
	}

	public virtual NtStatus SetEndOfFile(
		string fileName,
		long length,
		IDokanFileInfo info)
	{
		try
		{
			((FileStream)info.Context).SetLength(length);
			return DokanResult.Success;
		}
		catch (IOException)
		{
			return DokanResult.DiskFull;
		}
	}

	public virtual NtStatus SetAllocationSize(
		string fileName,
		long length,
		IDokanFileInfo info)
	{
		try
		{
			((FileStream)info.Context).SetLength(length);
			return DokanResult.Success;
		}
		catch (IOException)
		{
			return DokanResult.DiskFull;
		}
	}

	public virtual NtStatus LockFile(
		string fileName,
		long offset,
		long length,
		IDokanFileInfo info)
	{
		try
		{
			((FileStream)info.Context).Lock(offset, length);
			return DokanResult.Success;
		}
		catch (IOException)
		{
			return DokanResult.AccessDenied;
		}
	}

	public virtual NtStatus UnlockFile(
		string fileName,
		long offset,
		long length,
		IDokanFileInfo info)
	{
		try
		{
			((FileStream)info.Context).Unlock(offset, length);
			return DokanResult.Success;
		}
		catch (IOException)
		{
			return DokanResult.AccessDenied;
		}
	}

	public virtual NtStatus GetDiskFreeSpace(
		out long freeBytesAvailable,
		out long totalNumberOfBytes,
		out long totalNumberOfFreeBytes,
		IDokanFileInfo info)
	{
		freeBytesAvailable = FREE_SPACE;
		totalNumberOfBytes = FREE_SPACE;
		totalNumberOfFreeBytes = FREE_SPACE;
		return DokanResult.Success;
	}

	public virtual NtStatus GetVolumeInformation(
		out string volumeLabel,
		out FileSystemFeatures features,
		out string fileSystemName,
		out uint maximumComponentLength,
		IDokanFileInfo info)
	{
		volumeLabel = "Dokan Virtual Drive";
		fileSystemName = "Dokan Virtual FS";
		maximumComponentLength = 256;
		features = FileSystemFeatures.UnicodeOnDisk
			| FileSystemFeatures.CaseSensitiveSearch
			| FileSystemFeatures.PersistentAcls
			| FileSystemFeatures.SupportsRemoteStorage
			| FileSystemFeatures.CasePreservedNames;
		return DokanResult.Success;
	}

	public virtual NtStatus GetFileSecurity(
		string fileName,
		out FileSystemSecurity security,
		AccessControlSections sections,
		IDokanFileInfo info)
	{
		fileName = GetPath(fileName);
		security = info.IsDirectory
			? new DirectoryInfo(fileName).GetAccessControl()
			: new FileInfo(fileName).GetAccessControl();
		return DokanResult.Success;
	}

	public virtual NtStatus SetFileSecurity(
		string fileName,
		FileSystemSecurity security,
		AccessControlSections sections,
		IDokanFileInfo info)
	{
		fileName = GetPath(fileName);
		if (info.IsDirectory)
		{
			new DirectoryInfo(fileName)
				.SetAccessControl((DirectorySecurity)security);
		}
		else
		{
			new FileInfo(fileName)
				.SetAccessControl((FileSecurity)security);
		}
		return DokanResult.Success;
	}

	public virtual NtStatus Mounted(string mountPoint, IDokanFileInfo info)
	{
		this.mountPoint = mountPoint;
		return DokanResult.Success;
	}

	public virtual NtStatus Unmounted(IDokanFileInfo info)
	{
		return DokanResult.Success;
	}

	#endregion Implementation of IDokanOperations

	[LibraryImport("kernel32", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetFileTime(SafeFileHandle hFile, ref long lpCreationTime, ref long lpLastAccessTime, ref long lpLastWriteTime);
}
