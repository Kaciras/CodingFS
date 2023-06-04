using System.Reflection;
using DokanNet;

namespace CodingFS.Benchmark;

/// <summary>
/// 包装一个 IDonakOperations 对象，将一些常见的异常转换为 NtStatus 返回。
/// 
/// IDonakOperations 的方法如果抛出异常，则被视为实现出错，异常情况需要由返回的 NtStatus表示。
/// 在代理其他的文件系统时，但有些异常如找不到文件等与 NtStatus 有直接的对应关系，这里统一处理。
/// 
/// 因为 IDonakOperations 的方法太多，所以使用动态代理。
/// 经测试单次方法调用性能虽比静态的慢 100 倍，但仍不超过一微妙，可以忽略。
/// </summary>
public class AopFSWrapper : DispatchProxy
{
	public IDokanOperations Native { get; set; }

	protected override object? Invoke(MethodInfo method, object[] args)
	{
		try
		{
			return method.Invoke(Native, args);
		}
		catch (TargetInvocationException e)
		when (method.ReturnType == typeof(NtStatus))
		{
			return StaticFSWrapper.HandleException(e.InnerException!);
		}
	}

	/// <summary>
	/// 创建该动态代理的示例，包装指定的文件系统。
	/// </summary>
	/// <param name="fs">文件系统</param>
	/// <returns>包装后的代理对象</returns>
	public static IDokanOperations Create(IDokanOperations fs)
	{
		var instance = Create<IDokanOperations, AopFSWrapper>();
		((AopFSWrapper)instance).Native = fs;
		return instance; // 创建的代理实例同时属于 Create 的两个泛型参数的类型
	}
}
