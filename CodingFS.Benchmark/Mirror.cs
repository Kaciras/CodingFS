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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using DokanNet;
using DokanNet.Logging;
using static DokanNet.FormatProviders;
using FileAccess = DokanNet.FileAccess;

namespace CodingFS.Benchmark;

/// <summary>
/// Copied from https://github.com/dokan-dev/dokan-dotnet/blob/master/sample/DokanNetMirror/Mirror.cs
/// </summary>
internal class Mirror : IDokanOperations
{
	private readonly string path;

	private const FileAccess DataAccess = FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData |
										  FileAccess.Execute |
										  FileAccess.GenericExecute | FileAccess.GenericWrite |
										  FileAccess.GenericRead;

	private const FileAccess DataWriteAccess = FileAccess.WriteData | FileAccess.AppendData |
											   FileAccess.Delete |
											   FileAccess.GenericWrite;

	private readonly ILogger _logger;

	public Mirror(ILogger logger, string path)
	{
		if (!Directory.Exists(path))
			throw new ArgumentException(nameof(path));
		_logger = logger;
		this.path = path;
	}

	protected string GetPath(string fileName)
	{
		return path + fileName;
	}

	protected static Int32 GetNumOfBytesToCopy(Int32 bufferLength, long offset, IDokanFileInfo info, FileStream stream)
	{
		if (info.PagingIo)
		{
			var longDistanceToEnd = stream.Length - offset;
			var isDistanceToEndMoreThanInt = longDistanceToEnd > Int32.MaxValue;
			if (isDistanceToEndMoreThanInt) return bufferLength;
			var distanceToEnd = (Int32)longDistanceToEnd;
			if (distanceToEnd < bufferLength) return distanceToEnd;
			return bufferLength;
		}
		return bufferLength;
	}

	#region Implementation of IDokanOperations

	public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode,
		FileOptions options, FileAttributes attributes, IDokanFileInfo info)
	{
		var result = DokanResult.Success;
		var filePath = GetPath(fileName);

		if (info.IsDirectory)
		{
			try
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

						new DirectoryInfo(filePath).EnumerateFileSystemInfos().Any();
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
			catch (UnauthorizedAccessException)
			{
				return DokanResult.AccessDenied;
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
				pathExists = (Directory.Exists(filePath) || File.Exists(filePath));
				pathIsDirectory = pathExists ? File.GetAttributes(filePath).HasFlag(FileAttributes.Directory) : false;
			}
			catch (IOException)
			{
			}

			switch (mode)
			{
				case FileMode.Open:

					if (pathExists)
					{
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
					}
					else
					{
						return DokanResult.FileNotFound;
					}
					break;

				case FileMode.CreateNew:
					if (pathExists)
						return DokanResult.FileExists;
					break;

				case FileMode.Truncate:
					if (!pathExists)
						return DokanResult.FileNotFound;
					break;
			}

			try
			{
				System.IO.FileAccess streamAccess = readAccess ? System.IO.FileAccess.Read : System.IO.FileAccess.ReadWrite;

				if (mode == System.IO.FileMode.CreateNew && readAccess) streamAccess = System.IO.FileAccess.ReadWrite;

				info.Context = new FileStream(filePath, mode,
					streamAccess, share, 4096, options);

				if (pathExists && (mode == FileMode.OpenOrCreate
								   || mode == FileMode.Create))
					result = DokanResult.AlreadyExists;

				bool fileCreated = mode == FileMode.CreateNew || mode == FileMode.Create || (!pathExists && mode == FileMode.OpenOrCreate);
				if (fileCreated)
				{
					FileAttributes new_attributes = attributes;
					new_attributes |= FileAttributes.Archive; // Files are always created as Archive
															  // FILE_ATTRIBUTE_NORMAL is override if any other attribute is set.
					new_attributes &= ~FileAttributes.Normal;
					File.SetAttributes(filePath, new_attributes);
				}
			}
			catch (UnauthorizedAccessException) // don't have access rights
			{
				if (info.Context is FileStream fileStream)
				{
					// returning AccessDenied cleanup and close won't be called,
					// so we have to take care of the stream now
					fileStream.Dispose();
					info.Context = null;
				}
				return DokanResult.AccessDenied;
			}
			catch (DirectoryNotFoundException)
			{
				return DokanResult.PathNotFound;
			}
			catch (Exception ex)
			{
				var hr = (uint)Marshal.GetHRForException(ex);
				switch (hr)
				{
					case 0x80070020: //Sharing violation
						return DokanResult.SharingViolation;
					default:
						throw;
				}
			}
		}
		return result;
	}

	public void Cleanup(string fileName, IDokanFileInfo info)
	{
		(info.Context as FileStream)?.Dispose();
		info.Context = null;

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

	public void CloseFile(string fileName, IDokanFileInfo info)
	{
		(info.Context as FileStream)?.Dispose();
		info.Context = null;
		// could recreate cleanup code here but this is not called sometimes
	}

	public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
	{
		if (info.Context == null) // memory mapped read
		{
			using (var stream = new FileStream(GetPath(fileName), FileMode.Open, System.IO.FileAccess.Read))
			{
				stream.Position = offset;
				bytesRead = stream.Read(buffer, 0, buffer.Length);
			}
		}
		else // normal read
		{
			var stream = info.Context as FileStream;
			lock (stream) //Protect from overlapped read
			{
				stream.Position = offset;
				bytesRead = stream.Read(buffer, 0, buffer.Length);
			}
		}
		return DokanResult.Success;
	}

	public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
	{
		var append = offset == -1;
		if (info.Context == null)
		{
			using (var stream = new FileStream(GetPath(fileName), append ? FileMode.Append : FileMode.Open, System.IO.FileAccess.Write))
			{
				if (!append) // Offset of -1 is an APPEND: https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-writefile
				{
					stream.Position = offset;
				}
				var bytesToCopy = GetNumOfBytesToCopy(buffer.Length, offset, info, stream);
				stream.Write(buffer, 0, bytesToCopy);
				bytesWritten = bytesToCopy;
			}
		}
		else
		{
			var stream = info.Context as FileStream;
			lock (stream) //Protect from overlapped write
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
				var bytesToCopy = GetNumOfBytesToCopy(buffer.Length, offset, info, stream);
				stream.Write(buffer, 0, bytesToCopy);
				bytesWritten = bytesToCopy;
			}
		}
		return DokanResult.Success;
	}

	public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
	{
		try
		{
			((FileStream)(info.Context)).Flush();
			return DokanResult.Success;
		}
		catch (IOException)
		{
			return DokanResult.DiskFull;
		}
	}

	public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
	{
		// may be called with info.Context == null, but usually it isn't
		var filePath = GetPath(fileName);
		FileSystemInfo finfo = new FileInfo(filePath);
		if (!finfo.Exists)
			finfo = new DirectoryInfo(filePath);

		fileInfo = new FileInformation
		{
			FileName = fileName,
			Attributes = finfo.Attributes,
			CreationTime = finfo.CreationTime,
			LastAccessTime = finfo.LastAccessTime,
			LastWriteTime = finfo.LastWriteTime,
			Length = (finfo as FileInfo)?.Length ?? 0,
		};
		return DokanResult.Success;
	}

	public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
	{
		// This function is not called because FindFilesWithPattern is implemented
		// Return DokanResult.NotImplemented in FindFilesWithPattern to make FindFiles called
		files = FindFilesHelper(fileName, "*");

		return DokanResult.Success;
	}

	public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
	{
		try
		{
			// MS-FSCC 2.6 File Attributes : There is no file attribute with the value 0x00000000
			// because a value of 0x00000000 in the FileAttributes field means that the file attributes for this file MUST NOT be changed when setting basic information for the file
			if (attributes != 0)
				File.SetAttributes(GetPath(fileName), attributes);
			return DokanResult.Success;
		}
		catch (UnauthorizedAccessException)
		{
			return DokanResult.AccessDenied;
		}
		catch (FileNotFoundException)
		{
			return DokanResult.FileNotFound;
		}
		catch (DirectoryNotFoundException)
		{
			return DokanResult.PathNotFound;
		}
	}

	public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime,
		DateTime? lastWriteTime, IDokanFileInfo info)
	{
		try
		{
			if (info.Context is FileStream stream)
			{
				var ct = creationTime?.ToFileTime() ?? 0;
				var lat = lastAccessTime?.ToFileTime() ?? 0;
				var lwt = lastWriteTime?.ToFileTime() ?? 0;
				if (NativeMethods.SetFileTime(stream.SafeFileHandle, ref ct, ref lat, ref lwt))
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
		catch (UnauthorizedAccessException)
		{
			return DokanResult.AccessDenied;
		}
		catch (FileNotFoundException)
		{
			return DokanResult.FileNotFound;
		}
	}

	public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
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

	public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
	{
		return Directory.EnumerateFileSystemEntries(GetPath(fileName)).Any()
				? DokanResult.DirectoryNotEmpty
				: DokanResult.Success;
		// if dir is not empty it can't be deleted
	}

	public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
	{
		var oldpath = GetPath(oldName);
		var newpath = GetPath(newName);

		(info.Context as FileStream)?.Dispose();
		info.Context = null;

		var exist = info.IsDirectory ? Directory.Exists(newpath) : File.Exists(newpath);

		try
		{

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

				if (info.IsDirectory) //Cannot replace directory destination - See MOVEFILE_REPLACE_EXISTING
					return DokanResult.AccessDenied;

				File.Delete(newpath);
				File.Move(oldpath, newpath);
				return DokanResult.Success;
			}
		}
		catch (UnauthorizedAccessException)
		{
			return DokanResult.AccessDenied;
		}
		return DokanResult.FileExists;
	}

	public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
	{
		try
		{
			((FileStream)(info.Context)).SetLength(length);
			return DokanResult.Success;
		}
		catch (IOException)
		{
			return DokanResult.DiskFull;
		}
	}

	public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
	{
		try
		{
			((FileStream)(info.Context)).SetLength(length);
			return DokanResult.Success;
		}
		catch (IOException)
		{
			return DokanResult.DiskFull;
		}
	}

	public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
	{
		try
		{
			((FileStream)(info.Context)).Lock(offset, length);
			return DokanResult.Success;
		}
		catch (IOException)
		{
			return DokanResult.AccessDenied;
		}
	}

	public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
	{
		try
		{
			((FileStream)(info.Context)).Unlock(offset, length);
			return DokanResult.Success;
		}
		catch (IOException)
		{
			return DokanResult.AccessDenied;
		}
	}

	public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info)
	{
		var dinfo = DriveInfo.GetDrives().Single(di => string.Equals(di.RootDirectory.Name, Path.GetPathRoot(path + "\\"), StringComparison.OrdinalIgnoreCase));

		freeBytesAvailable = dinfo.TotalFreeSpace;
		totalNumberOfBytes = dinfo.TotalSize;
		totalNumberOfFreeBytes = dinfo.AvailableFreeSpace;
		return DokanResult.Success;
	}

	public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
		out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
	{
		volumeLabel = "DOKAN";
		fileSystemName = "NTFS";
		maximumComponentLength = 256;

		features = FileSystemFeatures.CasePreservedNames | FileSystemFeatures.CaseSensitiveSearch |
				   FileSystemFeatures.PersistentAcls | FileSystemFeatures.SupportsRemoteStorage |
				   FileSystemFeatures.UnicodeOnDisk;

		return DokanResult.Success;
	}

	public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
		IDokanFileInfo info)
	{
		try
		{
			security = info.IsDirectory
				? (FileSystemSecurity)new DirectoryInfo(GetPath(fileName)).GetAccessControl()
				: new FileInfo(GetPath(fileName)).GetAccessControl();
			return DokanResult.Success;
		}
		catch (UnauthorizedAccessException)
		{
			security = null;
			return DokanResult.AccessDenied;
		}
	}

	public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
		IDokanFileInfo info)
	{
		try
		{
			if (info.IsDirectory)
			{
				new DirectoryInfo(GetPath(fileName)).SetAccessControl((DirectorySecurity)security);
			}
			else
			{
				new FileInfo(GetPath(fileName)).SetAccessControl((FileSecurity)security);
			}
			return DokanResult.Success;
		}
		catch (UnauthorizedAccessException)
		{
			return DokanResult.AccessDenied;
		}
	}

	public NtStatus Mounted(string mountPoint, IDokanFileInfo info)
	{
		return DokanResult.Success;
	}

	public NtStatus Unmounted(IDokanFileInfo info)
	{
		return DokanResult.Success;
	}

	public NtStatus FindStreams(string fileName, IntPtr enumContext, out string streamName, out long streamSize,
		IDokanFileInfo info)
	{
		streamName = string.Empty;
		streamSize = 0;
		return DokanResult.NotImplemented;
	}

	public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
	{
		streams = new FileInformation[0];
		return DokanResult.NotImplemented;
	}

	public IList<FileInformation> FindFilesHelper(string fileName, string searchPattern)
	{
		IList<FileInformation> files = new DirectoryInfo(GetPath(fileName))
			.EnumerateFileSystemInfos()
			.Where(finfo => DokanHelper.DokanIsNameInExpression(searchPattern, finfo.Name, true))
			.Select(finfo => new FileInformation
			{
				Attributes = finfo.Attributes,
				CreationTime = finfo.CreationTime,
				LastAccessTime = finfo.LastAccessTime,
				LastWriteTime = finfo.LastWriteTime,
				Length = (finfo as FileInfo)?.Length ?? 0,
				FileName = finfo.Name
			}).ToArray();

		return files;
	}

	public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files,
		IDokanFileInfo info)
	{
		files = FindFilesHelper(fileName, searchPattern);

		return DokanResult.Success;
	}

	#endregion Implementation of IDokanOperations
}
