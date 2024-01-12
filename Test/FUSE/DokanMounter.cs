using System;
using System.IO;
using System.Threading;
using DokanNet;
using DokanNet.Logging;

namespace CodingFS.Test.FUSE;

public class DokanMounter : IDisposable
{
	public readonly string MountPoint;
	public readonly IDokanOperations VFS;

	readonly Dokan dokan;
	readonly DokanInstance instance;

	public DokanMounter(string point, IDokanOperations vfs, bool singleThread = false)
	{
		MountPoint = point;
		VFS = vfs;

		dokan = new Dokan(new NullLogger());
		instance = new DokanInstanceBuilder(dokan)
			.ConfigureOptions(options =>
			{
				options.MountPoint = point;
				options.SingleThread = singleThread;
			})
			.Build(vfs);
	}

	public virtual void Dispose()
	{
		instance.Dispose();
		dokan.Dispose();
		GC.SuppressFinalize(this);
	}

	public void WaitForReady()
	{
		var drive = new DriveInfo(MountPoint);
		for (int i = 0; i < 3000; i += 50)
		{
			Thread.Sleep(50);
			if (drive.IsReady)
				return;
		}
		throw new Exception("VFS mount timeout");
	}
}
