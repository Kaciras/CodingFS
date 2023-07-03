using System.IO;
using System.Threading;
using BenchmarkDotNet.Attributes;
using DokanNet;
using DokanNet.Logging;

namespace CodingFS.Benchmark;

file sealed class SafeImpl : RedirectDokan
{
	protected override string GetPath(string fileName)
	{
		return "unsafe-perf.data";
	}
}

file sealed class UnsafeImpl : UnsafeRedirectDokan
{
	protected override string GetPath(string fileName)
	{
		return "unsafe-perf.data";
	}
}

/// <summary>
/// 测试 IDokanOperationsUnsafe 的两个指针读写比非指针的快多少，结果:
/// |      Method |      Mean |     Error |    StdDev |
/// |------------ |----------:|----------:|----------:|
/// |  ReadUnsafe |  7.692 ms | 0.0711 ms | 0.0665 ms |
/// | ReadDefault | 10.674 ms | 0.2061 ms | 0.1928 ms |
/// </summary>
public class UnsafeDokanPerf
{
	Dokan dokan;
	DokanInstance unsafeFS;
	DokanInstance safeFS;

	[GlobalSetup]
	public void MountFileSystem()
	{
		dokan = new Dokan(new NullLogger());

		unsafeFS = new DokanInstanceBuilder(dokan)
			.ConfigureOptions(options => options.MountPoint = $"v:\\")
			.Build(new UnsafeImpl());

		safeFS = new DokanInstanceBuilder(dokan)
			.ConfigureOptions(options => options.MountPoint = $"w:\\")
			.Build(new SafeImpl());

		File.WriteAllBytes("unsafe-perf.data", new byte[10 * 1024 * 1024]);

		var driveV = new DriveInfo($"v:\\");
		var driveW = new DriveInfo($"w:\\");

		do { Thread.Sleep(50); }
		while (!driveV.IsReady || !driveW.IsReady);
	}

	[GlobalCleanup]
	public void UnmountFileSystem()
	{
		safeFS.Dispose();
		unsafeFS.Dispose();
		dokan.Dispose();
	}

	[Benchmark]
	public byte[] ReadUnsafe() => File.ReadAllBytes(@"v:\\data");

	[Benchmark]
	public byte[] ReadDefault() => File.ReadAllBytes(@"w:\\data");
}
