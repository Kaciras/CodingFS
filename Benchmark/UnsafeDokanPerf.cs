using System.IO;
using BenchmarkDotNet.Attributes;
using CodingFS.FUSE;
using CodingFS.Test.FUSE;

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
 * | Method     | Mean      | Error     | StdDev    | Ratio | RatioSD |
 * |----------- |----------:|----------:|----------:|------:|--------:|
 * | Direct     |  2.656 ms | 0.0307 ms | 0.0287 ms |  1.00 |    0.00 |
 * | Read       | 10.676 ms | 0.2119 ms | 0.4469 ms |  3.93 |    0.13 |
 * | ReadUnsafe |  7.807 ms | 0.0284 ms | 0.0237 ms |  2.94 |    0.03 |
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

	[Benchmark(Baseline = true)]
	public byte[] Direct() => File.ReadAllBytes("unsafe-perf.data");

	[Benchmark]
	public byte[] Read() => File.ReadAllBytes(@"w:\data");

	[Benchmark]
	public byte[] ReadUnsafe() => File.ReadAllBytes(@"v:\data");
}
