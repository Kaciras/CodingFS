using System.IO;
using BenchmarkDotNet.Attributes;
using CodingFS.VFS;
using DokanNet;

// 测试结果表明，动态代理在不出现异常的情况下比静态慢 23 倍，出现异常的情况下慢 3 倍。
// 不过即使这样，单个操作的时间仍在微秒级，影响并不大。
namespace CodingFS.Benchmark;

public class FSWrapperPerf
{
	private sealed class TestFS : DokanOperationBase
	{
		public override NtStatus Mounted(string _, IDokanFileInfo __)
		{
			throw new FileNotFoundException();
		}
	}

	readonly IDokanOperations staticProxy = new StaticFSWrapper(new TestFS());
	readonly IDokanOperations dynamicProxy = AopFSWrapper.Create(new TestFS());

	[Benchmark]
	public NtStatus Static_Throws()
	{
		return staticProxy.Mounted(string.Empty, null);
	}

	[Benchmark]
	public NtStatus Dynamic_Throws()
	{
		return dynamicProxy.Mounted(string.Empty, null);
	}

	[Benchmark]
	public NtStatus Static_Success()
	{
		return staticProxy.FindFiles("test", out _, null);
	}

	[Benchmark]
	public NtStatus Dynamic_Success()
	{
		return dynamicProxy.FindFiles("test", out _, null);
	}
}
