using System.IO;
using BenchmarkDotNet.Attributes;
using DokanNet;

// 测试结果表明，动态代理在不出现异常的情况下比静态慢100被，出现异常的情况下慢10倍。
// 不过即使这样，单个操作的时间仍在微秒级，影响并不大。
namespace CodingFS.Benchmark
{
	public class FSWrapperPerf
	{
		private sealed class TestFS : AbstractFileSystem
		{
			public override NtStatus Mounted(IDokanFileInfo _) => throw new FileNotFoundException();
		}

		private readonly IDokanOperations staticProxy = new StaticFSWrapper(new TestFS());
		private readonly IDokanOperations dynamicProxy = DynamicFSWrapper.Create(new TestFS());

		[Benchmark]
		public NtStatus Static_ReadFile_Throws()
		{
			return staticProxy.Mounted(null);
		}

		[Benchmark]
		public NtStatus Dynamic_ReadFile_Throws()
		{
			return dynamicProxy.Mounted(null);
		}

		[Benchmark]
		public NtStatus Static_ReadFile_Success()
		{
			return staticProxy.FindFiles("test", out _, null);
		}

		[Benchmark]
		public NtStatus Dynamic_ReadFile_Success()
		{
			return dynamicProxy.FindFiles("test", out _, null);
		}
	}
}
