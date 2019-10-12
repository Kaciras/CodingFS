using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DokanNet;

namespace CodingFS
{
	public class DynamicFSProxy : DispatchProxy
	{
		// 正常使用Create创建的话是不会为null的
		public IDokanOperations? Wrapped { get; set; }

		protected override object? Invoke(MethodInfo method, object[] args)
		{
			try
			{
				return method.Invoke(Wrapped, args);
			}
			catch (TargetInvocationException e)
			when (method.ReturnType == typeof(NtStatus))
			{
				return StaticFSProxy.HandleException(e.InnerException!);
			}
		}

		public static T Create<T>(T fs) where T : IDokanOperations
		{
			if (fs == null)
			{
				throw new ArgumentNullException();
			}
			var instance = Create<IDokanOperations, DynamicFSProxy>();
			((DynamicFSProxy)instance).Wrapped = fs;

			return (T)instance;
		}
	}
}
