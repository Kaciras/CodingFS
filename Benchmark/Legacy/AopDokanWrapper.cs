using System.Reflection;
using CodingFS.FUSE;
using DokanNet;

namespace CodingFS.Benchmark.Legacy;

public class AopDokanWrapper : DispatchProxy
{
	public IDokanOperations? Native { get; set; }

	protected override object? Invoke(MethodInfo method, object?[]? args)
	{
		try
		{
			return method.Invoke(Native, args);
		}
		catch (TargetInvocationException e)
		when (method.ReturnType == typeof(NtStatus))
		{
			return DokanExceptionWrapper.HandleException(e.InnerException!);
		}
	}

	public static IDokanOperations Create(IDokanOperations fs)
	{
		var instance = Create<IDokanOperations, AopDokanWrapper>();
		((AopDokanWrapper)instance).Native = fs;
		return instance; // 创建的代理实例同时属于 Create 的两个泛型参数的类型
	}
}
