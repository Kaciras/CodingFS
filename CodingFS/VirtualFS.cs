using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CodingFS.FUSE;
using DokanNet;
using DokanNet.Logging;

namespace CodingFS;

public struct VirtualFSOptions
{
	public string Name;

	public bool Readonly;

	public bool Debug;

	public FileType Type;

	public string? MountPoint;
}

public sealed class VirtualFS : IDisposable
{
	readonly List<IDisposable> disposables = new();

	public VirtualFS(Dictionary<string, CodingPathFilter> map, in VirtualFSOptions options)
	{
		if (OperatingSystem.IsWindows())
		{
			InitDokan(map, options);
		}
		else
		{
			throw new PlatformNotSupportedException();
		}
	}

	void InitDokan(Dictionary<string, CodingPathFilter> map, in VirtualFSOptions o)
	{
		var vfs = new FilteredDokan(o.Name)
		{
			Map = map,
			Type = o.Type,
		};
		DokanOptions mountOptions = default;
		ILogger dokanLogger;

		if (o.Readonly)
		{
			mountOptions |= DokanOptions.WriteProtection;
		}

		if (o.Debug)
		{
			dokanLogger = new ConsoleLogger("[Dokan] ");
			mountOptions |= DokanOptions.DebugMode | DokanOptions.StderrOutput;
			disposables.Add(Unsafe.As<IDisposable>(dokanLogger));
		}
		else
		{
			dokanLogger = new NullLogger();
		}

		var mountPoint = o.MountPoint;
		var dokan = new Dokan(dokanLogger);
		var builder = new DokanInstanceBuilder(dokan)
			.ConfigureOptions(options =>
			{
				options.MountPoint = mountPoint;
				options.Options = mountOptions;
			});

		disposables.Add(dokan);
		disposables.Add(builder.Build(new ExceptionWrapper(vfs)));
	}

	public void Dispose() => disposables.ForEach(x => x.Dispose());
}
