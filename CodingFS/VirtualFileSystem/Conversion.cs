using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using DokanNet;

namespace CodingFS.VirtualFileSystem
{
	public static class Conversion
	{
		public static FileInformation MapInfo(FileSystemInfo src) => new FileInformation
		{
			Attributes = src.Attributes,
			FileName = src.Name,
			LastAccessTime = src.LastAccessTime,
			CreationTime = src.CreationTime,
			LastWriteTime = src.LastWriteTime,
			Length = (src as FileInfo)?.Length ?? 0
		};

		public static FileInformation MapInfo(IFileSystemInfo src) => new FileInformation
		{
			Attributes = src.Attributes,
			FileName = src.Name,
			LastAccessTime = src.LastAccessTime,
			CreationTime = src.CreationTime,
			LastWriteTime = src.LastWriteTime,
			Length = (src as IFileInfo)?.Length ?? 0
		};
	}
}
