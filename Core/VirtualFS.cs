using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CodingFS.FUSE;
using DokanNet;
using DokanNet.Logging;

namespace CodingFS;

public struct VirtualFSOptions
{
	/// <summary>
	/// Volume label in Windows, default is "CodingFS".
	/// </summary>
	public string? Name;

	/// <summary>
	/// Mount the file system as read-only to avoid unexpected modification.
	/// Default is false.
	/// </summary>
	public bool Readonly;

	/// <summary>
	/// Print debug messages in console, default false.
	/// </summary>
	public bool Debug;

	/// <summary>
	/// The mount point of the file system, empty means choosed by CodingFS.
	/// </summary>
	public string MountPoint;
}

public sealed class VirtualFS : IDisposable
{
	readonly List<IDisposable> disposables = [];

	/// <summary>
	/// Create new instance of VirtualFS will mount the virtual volume.
	/// </summary>
	/// <exception cref="DllNotFoundException">
	/// If any required component not installed in the system.
	/// </exception>
	public VirtualFS(PathFilter filter, in VirtualFSOptions options)
	{
		if (OperatingSystem.IsWindows())
		{
			InitDokan(filter, options);
		}
		else
		{
			throw new PlatformNotSupportedException();
		}

		disposables.Reverse(); // Disposing order is the reverse of init order.
	}

	/// <summary>
	/// Dispose this object will unmount the file system.
	/// </summary>
	public void Dispose() => disposables.ForEach(x => x.Dispose());

	public static IEnumerable<string> GetFreeDrives()
	{
		var usedDrives = DriveInfo.GetDrives();
		return Enumerable.Range('A', 'Z' - 'A' + 1)
			.Select(i => (char)i + ":")
			.Except(usedDrives.Select(s => s.Name));
	}

	void InitDokan(PathFilter filter, in VirtualFSOptions o)
	{
		var vfs = new FilteredDokan(o.Name ?? "CodingFS", filter);
		var mountPoint = o.MountPoint;
		ILogger dokanLogger;
		DokanOptions mountFlags = default;

		if (mountPoint.Length == 0)
		{
			mountPoint = GetFreeDrives().First();
		}

		if (o.Readonly)
		{
			mountFlags |= DokanOptions.WriteProtection;
		}

		if (o.Debug)
		{
			dokanLogger = new ConsoleLogger("[Dokan] ");
			mountFlags |= DokanOptions.DebugMode | DokanOptions.StderrOutput;
			disposables.Add(Unsafe.As<IDisposable>(dokanLogger));
		}
		else
		{
			dokanLogger = new NullLogger();
		}

		var dokan = new Dokan(dokanLogger);

		// Why Dokan doesn't expose the options just as a property?
		var builder = new DokanInstanceBuilder(dokan).ConfigureOptions(options =>
		{
			options.Options = mountFlags;
			options.MountPoint = mountPoint;
		});

		disposables.Add(dokan);
		disposables.Add(builder.Build(new DokanExceptionWrapper(vfs)));
	}
}
