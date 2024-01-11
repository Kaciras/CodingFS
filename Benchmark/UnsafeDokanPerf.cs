using System.IO;
using System.Threading;
using BenchmarkDotNet.Attributes;
using CodingFS.FUSE;
using CodingFS.Test.FUSE;
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

/*
 * | Method     | Mean      | Error     | StdDev    | Median    |
 * |----------- |----------:|----------:|----------:|----------:|
 * | Read       | 11.192 ms | 0.2116 ms | 0.3167 ms | 11.102 ms |
 * | ReadUnsafe |  7.940 ms | 0.1551 ms | 0.1787 ms |  7.822 ms |
 */
[ReturnValueValidator]
public class UnsafeDokanPerf
{
	DokanMounter unsafeFS;
	DokanMounter safeFS;

	[GlobalSetup]
	public void Mount()
	{
		safeFS = new DokanMounter(@"w:", new SafeImpl());
		unsafeFS = new DokanMounter(@"v:", new UnsafeImpl());

		File.WriteAllBytes("unsafe-perf.data", new byte[10 * 1024 * 1024]);

		safeFS.WaitForReady();
		unsafeFS.WaitForReady();
	}

	[GlobalCleanup]
	public void Unmount()
	{
		safeFS.Dispose();
		unsafeFS.Dispose();
	}

	[Benchmark]
	public byte[] Read() => File.ReadAllBytes(@"w:\data");

	[Benchmark]
	public byte[] ReadUnsafe() => File.ReadAllBytes(@"v:\data");
}
