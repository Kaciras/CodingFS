using System;
using System.IO;
using System.Threading;
using DokanNet;
using DokanNet.Logging;

namespace CodingFS.Test.FUSE;

public sealed class DokanMounter : IDisposable
{
	readonly string mountPoint;
	Dokan dokan;
	DokanInstance instance;

	public DokanMounter(string point, IDokanOperations vfs)
	{
		mountPoint = point;
		dokan = new Dokan(new NullLogger());
		instance = new DokanInstanceBuilder(dokan)
			.ConfigureOptions(options => options.MountPoint = point)
			.Build(vfs);
	}

	public void Dispose()
	{
		instance.Dispose();
		dokan.Dispose();
	}

	public void WaitForReady()
	{
		var drive = new DriveInfo(mountPoint);
		for (int i = 0; i < 3000; i += 50)
		{
			Thread.Sleep(50);
			if (drive.IsReady)
				return;
		}
		throw new Exception("VFS mount timeout");
	}
}
