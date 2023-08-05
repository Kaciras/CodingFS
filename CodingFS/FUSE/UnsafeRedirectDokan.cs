using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using DokanNet;
using Microsoft.Win32.SafeHandles;
using AccessType = System.IO.FileAccess;

namespace CodingFS.FUSE;

abstract partial class UnsafeRedirectDokan : RedirectDokan, IDokanOperationsUnsafe
{
	public virtual NtStatus ReadFile(string fileName, IntPtr buffer, uint bufferLength,
		out int bytesRead, long offset, IDokanFileInfo info)
	{
		if (info.Context is FileStream reused)
		{
			lock (reused)
			{
				DoRead(reused.SafeFileHandle, buffer, bufferLength, out bytesRead, offset);
			}
		}
		else
		{
			using var stream = new FileStream(GetPath(fileName), FileMode.Open, AccessType.Read);
			DoRead(stream.SafeFileHandle, buffer, bufferLength, out bytesRead, offset);
		}
		return DokanResult.Success;
	}

	public virtual NtStatus WriteFile(string fileName, IntPtr buffer, uint bufferLength,
		out int bytesWritten, long offset, IDokanFileInfo info)
	{
		if (info.Context is FileStream reused)
		{
			lock (reused)
			{
				DoWrite(reused.SafeFileHandle, buffer, bufferLength, out bytesWritten, offset);
			}
		}
		else
		{
			using var stream = new FileStream(GetPath(fileName), FileMode.Open, AccessType.Write);
			DoWrite(stream.SafeFileHandle, buffer, bufferLength, out bytesWritten, offset);
		}
		return DokanResult.Success;
	}

	static void DoRead(SafeFileHandle handle, IntPtr buffer, uint length, out int read, long offset)
	{
		Check(SetFilePointerEx(handle, offset, IntPtr.Zero, 0));
		Check(ReadFile(handle, buffer, length, out read, IntPtr.Zero));
	}

	static void DoWrite(SafeFileHandle handle, IntPtr buffer, uint length, out int written, long offset)
	{
		Check(SetFilePointerEx(handle, offset, IntPtr.Zero, 0));
		Check(WriteFile(handle, buffer, length, out written, IntPtr.Zero));
	}

	static void Check(bool success)
	{
		if (!success) throw new Win32Exception();
	}

	[LibraryImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetFilePointerEx(SafeFileHandle hFile, long liDistanceToMove, IntPtr lpNewFilePointer, uint dwMoveMethod);

	[LibraryImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool ReadFile(SafeFileHandle hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, out int lpNumberOfBytesRead, IntPtr lpOverlapped);

	[LibraryImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool WriteFile(SafeFileHandle hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, IntPtr lpOverlapped);
}
