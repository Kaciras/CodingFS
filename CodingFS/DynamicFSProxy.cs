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
		public IDokanOperations Wrapped { get; set; }

		protected override object? Invoke(MethodInfo targetMethod, object[] args)
		{
			try
			{
				return targetMethod.Invoke(Wrapped, args);
			}
			catch (TargetInvocationException e) when (targetMethod.ReturnType == typeof(NtStatus))
			{
				return e.InnerException switch
				{
					FileNotFoundException _ => DokanResult.FileNotFound,
					DirectoryNotFoundException _ => DokanResult.FileNotFound,
					_ => throw e,
				};
			}
		}
	}
}
