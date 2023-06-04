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
using DokanNet;
using Microsoft.Win32.SafeHandles;
using AccessType = System.IO.FileAccess;
using FileAccess = DokanNet.FileAccess;

namespace CodingFS.Benchmark;

#pragma warning disable CA1416 // Unsupported operations will not be called.

/// <summary>
/// Copied from https://github.com/dokan-dev/dokan-dotnet/blob/master/sample/DokanNetMirror/Mirror.cs
/// </summary>
public abstract partial class RedirectFS : IDokanOperations
{
	const FileAccess DataAccess = FileAccess.Execute | FileAccess.GenericExecute
								| FileAccess.GenericWrite | FileAccess.GenericRead
								| FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData;

	const FileAccess DataWriteAccess = FileAccess.WriteData | FileAccess.AppendData
									 | FileAccess.Delete | FileAccess.GenericWrite;

	const long FREE_SPACE = 10 * 1024 * 1024 * 1024L;

	protected abstract string GetPath(string fileName);

	protected static FileInformation MapInfo(FileSystemInfo src) => new()
	{
		Attributes = src.Attributes,
		FileName = src.Name,
		LastAccessTime = src.LastAccessTime,
		CreationTime = src.CreationTime,
		LastWriteTime = src.LastWriteTime,
		Length = (src as FileInfo)?.Length ?? 0
	};

	protected static int GetNumOfBytesToCopy(int bufferLength, long offset, IDokanFileInfo info, FileStream stream)
	{
		if (info.PagingIo)
		{
			var longDistanceToEnd = stream.Length - offset;
			var isDistanceToEndMoreThanInt = longDistanceToEnd > int.MaxValue;
			if (isDistanceToEndMoreThanInt) return bufferLength;
			var distanceToEnd = (int)longDistanceToEnd;
			if (distanceToEnd < bufferLength) return distanceToEnd;
			return bufferLength;
		}
		return bufferLength;
	}

	#region Implementation of IDokanOperations

	public virtual NtStatus CreateFile(
		string fileName, FileAccess access, FileShare share, FileMode mode,
		FileOptions options, FileAttributes attributes, IDokanFileInfo info)
	{
		var result = DokanResult.Success;
		var filePath = GetPath(fileName);

		if (info.IsDirectory)
		{
			switch (mode)
			{
				case FileMode.Open:
					if (!Directory.Exists(filePath))
					{
						try
						{
							if (!File.GetAttributes(filePath).HasFlag(FileAttributes.Directory))
								return DokanResult.NotADirectory;
						}
						catch (Exception)
						{
							return DokanResult.FileNotFound;
						}
						return DokanResult.PathNotFound;
					}

					_ = new DirectoryInfo(filePath).EnumerateFileSystemInfos().Any();
					// you can't list the directory
					break;

				case FileMode.CreateNew:
					if (Directory.Exists(filePath))
						return DokanResult.FileExists;

					try
					{
						File.GetAttributes(filePath).HasFlag(FileAttributes.Directory);
						return DokanResult.AlreadyExists;
					}
					catch (IOException)
					{
					}

					Directory.CreateDirectory(GetPath(fileName));
					break;
			}
		}
		else
		{
			var pathExists = true;
			var pathIsDirectory = false;

			var readWriteAttributes = (access & DataAccess) == 0;
			var readAccess = (access & DataWriteAccess) == 0;

			try
			{
				pathExists = Directory.Exists(filePath) || File.Exists(filePath);
				pathIsDirectory = pathExists && File.GetAttributes(filePath).HasFlag(FileAttributes.Directory);
			}
			catch (IOException)
			{
			}

			switch (mode)
			{
				case FileMode.Open:

					if (!pathExists)
					{
						return DokanResult.FileNotFound;
					}

					// check if driver only wants to read attributes, security info, or open directory
					if (readWriteAttributes || pathIsDirectory)
					{
						if (pathIsDirectory && (access & FileAccess.Delete) == FileAccess.Delete
							&& (access & FileAccess.Synchronize) != FileAccess.Synchronize)
							//It is a DeleteFile request on a directory
							return DokanResult.AccessDenied;

						info.IsDirectory = pathIsDirectory;
						info.Context = new object();
						// must set it to something if you return DokanError.Success

						return DokanResult.Success;
					}
					break;

				case FileMode.CreateNew when pathExists:
					return DokanResult.FileExists;

				case FileMode.Truncate when !pathExists:
					return DokanResult.FileNotFound;
			}

			try
			{
				var streamAccess = readAccess ? AccessType.Read : AccessType.ReadWrite;

				if (mode == FileMode.CreateNew && readAccess) streamAccess = AccessType.ReadWrite;

				info.Context = new FileStream(filePath, mode,
					streamAccess, share, 4096, options);

				if (pathExists && (mode == FileMode.OpenOrCreate || mode == FileMode.Create))
					result = DokanResult.AlreadyExists;

				var fileCreated = mode == FileMode.CreateNew
					|| mode == FileMode.Create
					|| (!pathExists && mode == FileMode.OpenOrCreate);

				if (fileCreated)
				{
					FileAttributes new_attributes = attributes;
					new_attributes |= FileAttributes.Archive; // Files are always created as Archive
															  // FILE_ATTRIBUTE_NORMAL is override if any other attribute is set.
					new_attributes &= ~FileAttributes.Normal;
					File.SetAttributes(filePath, new_attributes);
				}
			}
			catch (UnauthorizedAccessException)
			{
				// returning AccessDenied cleanup and close won't be called,
				// so we have to take care of the stream now.
				CloseFile(fileName, info);
				return DokanResult.AccessDenied;
			}
		}
		return result;
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
		// could recreate cleanup code here but this is not called sometimes
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
			var stream = info.Context as FileStream;
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
			bytesWritten = GetNumOfBytesToCopy(buffer.Length, offset, info, stream);
			stream.Write(buffer, 0, bytesWritten);
		}
		else
		{
			var stream = info.Context as FileStream;
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
				bytesWritten = GetNumOfBytesToCopy(buffer.Length, offset, info, stream);
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
		// 这个根文件夹必须要有，否则会出现许多奇怪的错误。
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
			var rawPath = GetPath(fileName);
			FileSystemInfo rawInfo = new FileInfo(rawPath);

			if (rawInfo.Exists)
			{
				fileInfo = MapInfo(rawInfo);
			}
			else
			{
				rawInfo = new DirectoryInfo(rawPath);
				if (rawInfo.Exists)
				{
					fileInfo = MapInfo(rawInfo);
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

	public virtual NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
	{
		// MS-FSCC 2.6 File Attributes : There is no file attribute with the value 0x00000000
		// because a value of 0x00000000 in the FileAttributes field means that the file attributes for this file MUST NOT be changed when setting basic information for the file
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
			throw Marshal.GetExceptionForHR(Marshal.GetLastWin32Error());
		}

		var filePath = GetPath(fileName);

		if (creationTime.HasValue)
			File.SetCreationTime(filePath, creationTime.Value);

		if (lastAccessTime.HasValue)
			File.SetLastAccessTime(filePath, lastAccessTime.Value);

		if (lastWriteTime.HasValue)
			File.SetLastWriteTime(filePath, lastWriteTime.Value);

		return DokanResult.Success;
	}

	public virtual NtStatus DeleteFile(string fileName, IDokanFileInfo info)
	{
		var filePath = GetPath(fileName);

		if (Directory.Exists(filePath))
			return DokanResult.AccessDenied;

		if (!File.Exists(filePath))
			return DokanResult.FileNotFound;

		if (File.GetAttributes(filePath).HasFlag(FileAttributes.Directory))
			return DokanResult.AccessDenied;

		return DokanResult.Success;
		// we just check here if we could delete the file - the true deletion is in Cleanup
	}

	public virtual NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
	{
		// if dir is not empty it can't be deleted
		return Directory.EnumerateFileSystemEntries(GetPath(fileName)).Any()
				? DokanResult.DirectoryNotEmpty : DokanResult.Success;
	}

	public virtual NtStatus MoveFile(
		string oldName,
		string newName,
		bool replace,
		IDokanFileInfo info)
	{
		var oldpath = GetPath(oldName);
		var newpath = GetPath(newName);

		CloseFile(oldName, info);

		var exist = info.IsDirectory ? Directory.Exists(newpath) : File.Exists(newpath);
		if (!exist)
		{
			info.Context = null;
			if (info.IsDirectory)
				Directory.Move(oldpath, newpath);
			else
				File.Move(oldpath, newpath);
			return DokanResult.Success;
		}
		else if (replace)
		{
			info.Context = null;

			// Cannot replace directory destination - See MOVEFILE_REPLACE_EXISTING
			if (info.IsDirectory)
				return DokanResult.AccessDenied;

			File.Delete(newpath);
			File.Move(oldpath, newpath);
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
		security = info.IsDirectory
			? new DirectoryInfo(GetPath(fileName)).GetAccessControl()
			: new FileInfo(GetPath(fileName)).GetAccessControl();
		return DokanResult.Success;
	}

	public virtual NtStatus SetFileSecurity(
		string fileName,
		FileSystemSecurity security,
		AccessControlSections sections,
		IDokanFileInfo info)
	{
		if (info.IsDirectory)
		{
			new DirectoryInfo(GetPath(fileName))
				.SetAccessControl((DirectorySecurity)security);
		}
		else
		{
			new FileInfo(GetPath(fileName))
				.SetAccessControl((FileSecurity)security);
		}
		return DokanResult.Success;
	}

	public virtual NtStatus Mounted(string mountPoint, IDokanFileInfo info)
	{
		return DokanResult.Success;
	}

	public virtual NtStatus Unmounted(IDokanFileInfo info)
	{
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
			.Select(MapInfo).ToArray();
	}

	public virtual NtStatus FindFilesWithPattern(
		string fileName,
		string searchPattern,
		out IList<FileInformation> files,
		IDokanFileInfo info)
	{
		files = FindFilesHelper(fileName, searchPattern);
		return DokanResult.Success;
	}

	#endregion Implementation of IDokanOperations

	[LibraryImport("kernel32", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetFileTime(SafeFileHandle hFile, ref long lpCreationTime, ref long lpLastAccessTime, ref long lpLastWriteTime);
}
