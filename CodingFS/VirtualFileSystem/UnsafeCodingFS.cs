using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using DokanNet;
using Microsoft.Win32.SafeHandles;
using AccessType = System.IO.FileAccess;

namespace CodingFS.VirtualFileSystem
{
	/// <summary>
	/// 相比于非Unsafe的实现，该实现使用IntPtr指针操作底层缓冲，避免额外的复制消耗。
	/// 
	/// 这个文件的代码参考了官方的示例：
	/// https://github.com/dokan-dev/dokan-dotnet/blob/master/sample/DokanNetMirror/UnsafeMirror.cs
	/// </summary>
	internal class UnsafeCodingFS : CodingFS, IDokanOperationsUnsafe
	{
		public UnsafeCodingFS(FileType type, Dictionary<string, FileClassifier> map) : base(type, map) { }

		public NtStatus ReadFile(
			string fileName,
			IntPtr buffer,
			uint bufferLength,
			out int bytesRead,
			long offset,
			IDokanFileInfo info)
		{
			if (info.Context == null)
			{
				using var stream = new FileStream(MapPath(fileName), FileMode.Open, AccessType.Read);
				DoRead(stream, buffer, bufferLength, out bytesRead, offset);
			}
			else
			{
				var stream = (FileStream)info.Context;
				lock (stream)
				{
					DoRead(stream, buffer, bufferLength, out bytesRead, offset);
				}
			}
			return DokanResult.Success;
		}

		private static void DoRead(
			FileStream stream,
			IntPtr innerBuffer,
			uint innerBufferLength,
			out int innerBytesRead,
			long innerOffset)
		{
			SetFilePointer(stream.SafeFileHandle, innerOffset);
			ReadFile(stream.SafeFileHandle, innerBuffer, innerBufferLength, out innerBytesRead);
		}

		public NtStatus WriteFile(
			string fileName,
			IntPtr buffer,
			uint bufferLength,
			out int bytesWritten,
			long offset,
			IDokanFileInfo info)
		{
			if (info.Context == null)
			{
				using var stream = new FileStream(MapPath(fileName), FileMode.Open, AccessType.Write);
				DoWrite(stream, buffer, bufferLength, out bytesWritten, offset);
			}
			else
			{
				var stream = (FileStream)info.Context;
				lock (stream)
				{
					DoWrite(stream, buffer, bufferLength, out bytesWritten, offset);
				}
			}
			return DokanResult.Success;
		}

		private static void DoWrite(
			FileStream stream,
			IntPtr innerBuffer,
			uint innerBufferLength,
			out int innerBytesWritten,
			long innerOffset)
		{
			SetFilePointer(stream.SafeFileHandle, innerOffset);
			WriteFile(stream.SafeFileHandle, innerBuffer, innerBufferLength, out innerBytesWritten);
		}

		public static void SetFilePointer(SafeFileHandle fileHandle, long offset)
		{
			if (!SetFilePointerEx(fileHandle, offset, IntPtr.Zero, 0))
			{
				throw new Win32Exception();
			}
		}

		public static void ReadFile(SafeFileHandle fileHandle, IntPtr buffer, uint bytesToRead, out int bytesRead)
		{
			if (!ReadFile(fileHandle, buffer, bytesToRead, out bytesRead, IntPtr.Zero))
			{
				throw new Win32Exception();
			}
		}

		public static void WriteFile(SafeFileHandle fileHandle, IntPtr buffer, uint bytesToWrite, out int bytesWritten)
		{
			if (!WriteFile(fileHandle, buffer, bytesToWrite, out bytesWritten, IntPtr.Zero))
			{
				throw new Win32Exception();
			}
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetFilePointerEx(SafeFileHandle hFile, long liDistanceToMove, IntPtr lpNewFilePointer, uint dwMoveMethod);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool ReadFile(SafeFileHandle hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, out int lpNumberOfBytesRead, IntPtr lpOverlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool WriteFile(SafeFileHandle hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, IntPtr lpOverlapped);
	}
}
