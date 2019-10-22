﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DokanNet;

namespace CodingFS
{
	/// <summary>
	/// 包装一个IDonakOperations对象，将一些常见的异常转换为NtStatus返回。
	/// 
	/// IDonakOperations的方法如果抛出异常，则被视为实现出错，异常情况需要由返回的NtStatus表示。
	/// 在代理其他的文件系统时，但有些异常如找不到文件等与NtStatus有直接的对应关系，这里统一处理。
	/// 
	/// 因为IDonakOperations的方法太多，所以使用动态代理。
	/// </summary>
	public class DynamicFSProxy : DispatchProxy
	{
		// 正常使用Create创建是不会为null的
#pragma warning disable CS8618
		public IDokanOperations Native { get; set; }
#pragma warning restore CS8618

		protected override object? Invoke(MethodInfo method, object[] args)
		{
			try
			{
				return method.Invoke(Native, args);
			}
			catch (TargetInvocationException e)
			when (method.ReturnType == typeof(NtStatus))
			{
				return StaticFSProxy.HandleException(e.InnerException!);
			}
		}

		/// <summary>
		/// 创建该动态代理的示例，包装指定的文件系统。
		/// </summary>
		/// <typeparam name="T">被包装的类型</typeparam>
		/// <param name="fs">文件系统</param>
		/// <returns>包装后的代理对象</returns>
		public static T Create<T>(T fs) where T : IDokanOperations
		{
			if (fs == null)
			{
				throw new ArgumentNullException();
			}
			var instance = Create<IDokanOperations, DynamicFSProxy>();
			((DynamicFSProxy)instance).Native = fs;

			return (T)instance;
		}
	}
}
