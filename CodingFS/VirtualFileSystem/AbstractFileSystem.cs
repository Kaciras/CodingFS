using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using DokanNet;
using AccessType = System.IO.FileAccess;
using FileAccess = DokanNet.FileAccess;

namespace CodingFS.VirtualFileSystem
{
	public class AbstractFileSystem : IDokanOperations
	{
		const FileAccess DATA_ACCESS = FileAccess.AppendData
									 | FileAccess.WriteData
									 | FileAccess.ReadData
									 | FileAccess.Execute
									 | FileAccess.GenericWrite
									 | FileAccess.GenericRead
									 | FileAccess.GenericExecute;

		const FileAccess WRITE_ACCESS = FileAccess.AppendData
									  | FileAccess.WriteData
									  | FileAccess.Delete
									  | FileAccess.GenericWrite;

		private readonly IFileSystem native;

		public AbstractFileSystem(IFileSystem native)
		{
			this.native = native;
		}

		public NtStatus CreateFile(
			string fileName,
			FileAccess access,
			FileShare share,
			FileMode mode,
			FileOptions options,
			FileAttributes attributes,
			IDokanFileInfo info)
		{
			var result = DokanResult.Success;

			if (info.IsDirectory)
			{
				switch (mode)
				{
					case FileMode.Open:
						if (!Directory.Exists(fileName))
						{
							try
							{
								if (!File.GetAttributes(fileName).HasFlag(FileAttributes.Directory))
									return DokanResult.NotADirectory;
							}
							catch (Exception)
							{
								return DokanResult.FileNotFound;
							}
							return DokanResult.PathNotFound;
						}

						new DirectoryInfo(fileName).EnumerateFileSystemInfos().Any();
						// you can't list the directory
						break;

					case FileMode.CreateNew:
						if (Directory.Exists(fileName))
						{
							return DokanResult.FileExists;
						}
						try
						{
							File.GetAttributes(fileName).HasFlag(FileAttributes.Directory);
							return DokanResult.AlreadyExists;
						}
						catch (IOException)
						{
						}
						Directory.CreateDirectory(fileName);
						break;
				}
			}
			else
			{
				var pathExists = true;
				var pathIsDirectory = false;

				var readWriteAttributes = (access & DATA_ACCESS) == 0;
				var readAccess = (access & WRITE_ACCESS) == 0;

				try
				{
					pathExists = (Directory.Exists(fileName) || File.Exists(fileName));
					pathIsDirectory = pathExists ? File.GetAttributes(fileName).HasFlag(FileAttributes.Directory) : false;
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
								// must set it to someting if you return DokanResult.Success

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
					info.Context = new FileStream(fileName, mode,
						readAccess ? AccessType.Read : AccessType.ReadWrite, share, 4096, options);

					if (pathExists && (mode == FileMode.OpenOrCreate || mode == FileMode.Create))
					{
						result = DokanResult.AlreadyExists;
					}

					// Files are always created as Archive
					if (mode == FileMode.CreateNew || mode == FileMode.Create)
					{
						attributes |= FileAttributes.Archive;
					}

					File.SetAttributes(fileName, attributes);
				}
				catch (UnauthorizedAccessException)
				{
					// returning AccessDenied cleanup and close won't be called,
					// so we have to take care of the stream now
					if (info.Context is FileStream fileStream)
					{
						fileStream.Dispose();
						info.Context = null;
					}
					return DokanResult.AccessDenied;
				}
				catch (Exception ex)
				{
					var hr = (uint)Marshal.GetHRForException(ex);
					switch (hr)
					{
						case 0x80070020:
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
			CloseFile(fileName, info);

			if (info.DeleteOnClose)
			{
				if (info.IsDirectory)
				{
					native.Directory.Delete(fileName);
				}
				else
				{
					native.File.Delete(fileName);
				}
			}
		}

		public void CloseFile(string fileName, IDokanFileInfo info)
		{
			(info.Context as Stream)?.Dispose();
			info.Context = null;
		}

		public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
		{
			native.Directory.Delete(fileName);
			return DokanResult.Success;
		}

		public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
		{
			native.File.Delete(fileName);
			return DokanResult.Success;
		}

		public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
		{
			return FindFilesWithPattern(fileName, "*", out files, info);
		}

		public NtStatus FindFilesWithPattern(
			string fileName,
			string pattern,
			out IList<FileInformation> files,
			IDokanFileInfo info)
		{
			files = native.DirectoryInfo.FromDirectoryName(fileName)
					.EnumerateFileSystemInfos()
					.Where(finfo => DokanHelper.DokanIsNameInExpression(pattern, finfo.Name, true))
					.Select(Conversion.MapInfo).ToArray();
			return DokanResult.Success;
		}

		public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
		{
			streams = new FileInformation[0];
			return DokanResult.NotImplemented;
		}

		public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
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

		public NtStatus GetDiskFreeSpace(
			out long freeBytesAvailable,
			out long totalNumberOfBytes,
			out long totalNumberOfFreeBytes,
			IDokanFileInfo info)
		{
			const long FREE = 10 * 1024 * 1024 * 1024L;
			freeBytesAvailable = FREE;
			totalNumberOfBytes = FREE;
			totalNumberOfFreeBytes = FREE;
			return DokanResult.Success;
		}

		public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
		{
			IFileSystemInfo finfo = native.FileInfo.FromFileName(fileName);
			if (!finfo.Exists)
			{
				finfo = native.DirectoryInfo.FromDirectoryName(fileName);
			}
			fileInfo = Conversion.MapInfo(finfo);
			return DokanResult.Success;
		}

		public NtStatus GetFileSecurity(
			string fileName,
			out FileSystemSecurity? security,
			AccessControlSections sections,
			IDokanFileInfo info)
		{
			security = default;
			return DokanResult.NotImplemented;
		}

		public NtStatus GetVolumeInformation(
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

		public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
		{
			try
			{
				(info.Context as FileStream)?.Lock(offset, length);
				return DokanResult.Success;
			}
			catch (IOException)
			{
				return DokanResult.AccessDenied;
			}
		}

		public NtStatus Mounted(IDokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
		{
			if (info.Context == null)
			{
				using var stream = native.FileStream.Create(fileName, FileMode.Open, AccessType.Read);
				stream.Position = offset;
				bytesRead = stream.Read(buffer, 0, buffer.Length);
			}
			else
			{
				var stream = (Stream)info.Context;
				lock (stream)
				{
					stream.Position = offset;
					bytesRead = stream.Read(buffer, 0, buffer.Length);
				}
			}
			return DokanResult.Success;
		}

		public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
		{
			return SetEndOfFile(fileName, length, info);
		}

		public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
		{
			try
			{
				(info.Context as FileStream)?.SetLength(length);
				return DokanResult.Success;
			}
			catch (IOException)
			{
				return DokanResult.DiskFull;
			}
		}

		public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
		{
			if (attributes != 0)
			{
				native.File.SetAttributes(fileName, attributes);
			}
			return DokanResult.Success;
		}

		public NtStatus SetFileSecurity(
			string fileName,
			FileSystemSecurity security,
			AccessControlSections sections,
			IDokanFileInfo info)
		{
			if (info.IsDirectory)
			{
				native.Directory.SetAccessControl(fileName, (DirectorySecurity)security);
			}
			else
			{
				native.File.SetAccessControl(fileName, (FileSecurity)security);
			}
			return DokanResult.Success;
		}

		public NtStatus SetFileTime(
			string fileName,
			DateTime? creationTime,
			DateTime? lastAccessTime,
			DateTime? lastWriteTime,
			IDokanFileInfo info)
		{
			if (creationTime.HasValue)
			{
				File.SetCreationTime(fileName, creationTime.Value);
			}
			if (lastAccessTime.HasValue)
			{
				File.SetLastAccessTime(fileName, lastAccessTime.Value);
			}
			if (lastWriteTime.HasValue)
			{
				File.SetLastWriteTime(fileName, lastWriteTime.Value);
			}
			return DokanResult.Success;
		}

		public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
		{
			try
			{
				(info.Context as FileStream)?.Unlock(offset, length);
				return DokanResult.Success;
			}
			catch (IOException)
			{
				return DokanResult.AccessDenied;
			}
		}

		public NtStatus Unmounted(IDokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
		{
			if (info.Context == null)
			{
				using var stream = native.FileStream.Create(fileName, FileMode.Open, AccessType.Write);
				stream.Position = offset;
				stream.Write(buffer, 0, buffer.Length);
			}
			else
			{
				var stream = (Stream)info.Context;
				lock (stream)
				{
					stream.Position = offset;
					stream.Write(buffer, 0, buffer.Length);
				}
			}
			bytesWritten = buffer.Length;
			return DokanResult.Success;
		}
	}
}
