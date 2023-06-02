using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using BenchmarkDotNet.Attributes;
using CodingFS.VFS;
using DokanNet;
using DokanNet.Logging;
using Microsoft.Win32.SafeHandles;
using AccessType = System.IO.FileAccess;

namespace CodingFS.Benchmark;

file sealed class SafeImpl : DokanOperationBase
{
	public override NtStatus GetFileInformation(
		string fileName,
		out FileInformation fileInfo,
		IDokanFileInfo info)
	{
		if (fileName == @"\")
		{
			fileInfo = new FileInformation
			{
				FileName = fileName,
				Length = 0,
				Attributes = FileAttributes.Directory,
			};
			return DokanResult.Success;
		}

		var time = DateTime.Now;
		fileInfo = new FileInformation
		{
			FileName = fileName,
			Length = 10*1024*1024,
			Attributes = FileAttributes.Normal,
			CreationTime = time,
			LastAccessTime = time,
			LastWriteTime = time,
		};
		return DokanResult.Success;
	}

	public override NtStatus ReadFile(
		string fileName,
		byte[] buffer,
		out int bytesRead,
		long offset,
		IDokanFileInfo info)
	{
		if (info.Context == null)
		{
			// FileAccess 默认是 ReadWrite，会造成额外的锁定
			using var stream = new FileStream("unsafe-perf.data", FileMode.Open, AccessType.Read)
			{
				Position = offset,
			};
			bytesRead = stream.Read(buffer, 0, buffer.Length);
		}
		else
		{
			var stream = (FileStream)info.Context;

			// Protect from overlapped read
			lock (stream)
			{
				stream.Position = offset;
				bytesRead = stream.Read(buffer, 0, buffer.Length);
			}
		}
		return DokanResult.Success;
	}
}

sealed partial class UnsafeImpl : DokanOperationBase, IDokanOperationsUnsafe
{
	public override NtStatus GetFileInformation(
		string fileName,
		out FileInformation fileInfo,
		IDokanFileInfo info)
	{
		if (fileName == @"\")
		{
			fileInfo = new FileInformation
			{
				FileName = fileName,
				Length = 0,
				Attributes = FileAttributes.Directory,
			};
			return DokanResult.Success;
		}

		var time = DateTime.Now;
		fileInfo = new FileInformation
		{
			FileName = fileName,
			Length = 10 * 1024 * 1024,
			Attributes = FileAttributes.Normal,
			CreationTime = time,
			LastAccessTime = time,
			LastWriteTime = time,
		};
		return DokanResult.Success;
	}

	public NtStatus ReadFile(
		string fileName,
		nint buffer,
		uint bufferLength, 
		out int bytesRead, 
		long offset,
		IDokanFileInfo info)
	{
		if (info.Context == null)
		{
			using var stream = new FileStream("unsafe-perf.data", FileMode.Open, AccessType.Read);
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
		nint buffer,
		uint bufferLength,
		out int bytesWritten,
		long offset, 
		IDokanFileInfo info)
	{
		throw new NotImplementedException();
	}

	private static void DoRead(SafeFileHandle handle,
		IntPtr buffer, uint length, out int read, long offset)
	{
		Check(SetFilePointerEx(handle, offset, IntPtr.Zero, 0));
		Check(ReadFile(handle, buffer, length, out read, IntPtr.Zero));
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

	private static void Check(bool success)
	{
		if (!success) throw new Win32Exception();
	}
}

/// <summary>
/// 测试 IDokanOperationsUnsafe 的两个指针读写比非指针的快多少，结果:
/// |      Method |     Mean |     Error |    StdDev |
/// |------------ |---------:|----------:|----------:|
/// |  ReadUnsafe | 6.310 ms | 0.0304 ms | 0.0269 ms |
/// | ReadDefault | 9.157 ms | 0.0800 ms | 0.0748 ms |
/// </summary>
public class FSUnsafePerf
{
	Dokan dokan;
	DokanInstance unsafeFS;
	DokanInstance safeFS;

	[GlobalSetup]
	public void MountFileSystem()
	{
		var mountOptions = DokanOptions.DebugMode | DokanOptions.MountManager | DokanOptions.CurrentSession;
		var dokanLogger = new NullLogger();

		dokan = new Dokan(dokanLogger);
		unsafeFS = new DokanInstanceBuilder(dokan)
			.ConfigureOptions(options =>
			{
				options.MountPoint = $"v:\\";
				options.Options = mountOptions;
			})
			.Build(new UnsafeImpl());

		safeFS = new DokanInstanceBuilder(dokan)
			.ConfigureOptions(options =>
			{
				options.MountPoint = $"w:\\";
				options.Options = mountOptions;
			})
			.Build(new SafeImpl());

		File.WriteAllBytes("unsafe-perf.data", new byte[10 * 1024 * 1024]);

		var drive = new DriveInfo($"v:\\");
		var drive2 = new DriveInfo($"w:\\");
		while (!drive.IsReady || !drive2.IsReady)
			Thread.Sleep(50);
	}

	[GlobalCleanup]
	public void UnmountFileSystem()
	{
		safeFS.Dispose();
		unsafeFS.Dispose();
		dokan.Dispose();
	}

	[Benchmark]
	public byte[] ReadUnsafe()
	{
		return File.ReadAllBytes(@"v:\\data");
	}

	[Benchmark]
	public byte[] ReadDefault()
	{
		return File.ReadAllBytes(@"w:\\data");
	}
}
