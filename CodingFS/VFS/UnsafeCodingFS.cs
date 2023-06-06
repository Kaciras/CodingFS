using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using DokanNet;
using Microsoft.Win32.SafeHandles;
using AccessType = System.IO.FileAccess;

namespace CodingFS.VFS;

/// <summary>
/// 相比于非Unsafe的实现，该实现使用 IntPtr 指针操作底层缓冲，避免额外的复制消耗。
/// 
/// 这个文件的代码参考了官方的示例：
/// https://github.com/dokan-dev/dokan-dotnet/blob/master/sample/DokanNetMirror/UnsafeMirror.cs
/// </summary>
internal partial class UnsafeCodingFS : CodingFS, IDokanOperationsUnsafe
{
	public UnsafeCodingFS(string name) : base(name) {}

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
			using var stream = new FileStream(GetPath(fileName), FileMode.Open, AccessType.Read);
			DoRead(stream.SafeFileHandle, buffer, bufferLength, out bytesRead, offset);
		}
		else
		{
			var stream = (FileStream)info.Context;
			lock (stream)
			{
				DoRead(stream.SafeFileHandle, buffer, bufferLength, out bytesRead, offset);
			}
		}
		return DokanResult.Success;
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
			using var stream = new FileStream(GetPath(fileName), FileMode.Open, AccessType.Write);
			DoWrite(stream.SafeFileHandle, buffer, bufferLength, out bytesWritten, offset);
		}
		else
		{
			var stream = (FileStream)info.Context;
			lock (stream)
			{
				DoWrite(stream.SafeFileHandle, buffer, bufferLength, out bytesWritten, offset);
			}
		}
		return DokanResult.Success;
	}

	private static void DoRead(SafeFileHandle handle, IntPtr buffer, uint length, out int read, long offset)
	{
		Check(SetFilePointerEx(handle, offset, IntPtr.Zero, 0));
		Check(ReadFile(handle, buffer, length, out read, IntPtr.Zero));
	}

	private static void DoWrite(SafeFileHandle handle, IntPtr buffer, uint length, out int written, long offset)
	{
		Check(SetFilePointerEx(handle, offset, IntPtr.Zero, 0));
		Check(WriteFile(handle, buffer, length, out written, IntPtr.Zero));
	}


	[LibraryImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetFilePointerEx(
		SafeFileHandle hFile,
		long liDistanceToMove,
		IntPtr lpNewFilePointer,
		uint dwMoveMethod);

	[LibraryImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool ReadFile(
		SafeFileHandle hFile,
		IntPtr lpBuffer,
		uint nNumberOfBytesToRead,
		out int lpNumberOfBytesRead,
		IntPtr lpOverlapped);

	[LibraryImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool WriteFile(
		SafeFileHandle hFile,
		IntPtr lpBuffer,
		uint nNumberOfBytesToWrite,
		out int lpNumberOfBytesWritten,
		IntPtr lpOverlapped);

	private static void Check(bool success)
	{
		if (!success) throw new Win32Exception();
	}
}
