using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace CodingFS;

/// <summary>
/// 如果要用路径作为字典的键，则有两个问题：统一Win和Unix分隔符、忽略尾部分隔符。
/// 如果直接处理字符串，则需要堆分配，使用此类包装一下，修改Equals和HashCode即可避免。
/// 
/// 之所以不直接用HashCode作为键有两个原因：
/// 1）整数Hash冲突时无法比较原字符串来判断是否相等。
/// 2）整数不利于调试，在IDE里看不到原始值。
/// </summary>
[DebuggerDisplay("FilePath:{Value}")]
public readonly struct FilePath
{
	public readonly string Value { get; }

	public FilePath(string value)
	{
		Value = value;
	}

	public override bool Equals(object? obj)
	{
		if (obj is not FilePath path)
		{
			return false;
		}

		var thisVal = Path.TrimEndingDirectorySeparator(Value.AsSpan());
		var objVal = Path.TrimEndingDirectorySeparator(path.Value.AsSpan());

		if (thisVal.Length != objVal.Length)
		{
			return false;
		}

		for (var i = 0; i < thisVal.Length; i++)
		{
			var a = thisVal[i];
			var b = objVal[i];

			if (a == b)
			{
				continue;
			}
			if (IsSep(a) && IsSep(b))
			{
				continue;
			}
			return false;
		}

		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsSep(char c)
	{
		return c == '\\' || c == '/';
	}

	// [-30ns] 使用 Span 版的 TrimEndingDirectorySeparator
	// [+10ns] 循环改成减值迭代反而更慢了
	public override int GetHashCode()
	{
		var hash = 80059289;
		var trimed = Path.TrimEndingDirectorySeparator(Value.AsSpan());

		foreach (var c in trimed)
		{
			hash = 31 * hash + (c == '\\' ? '/' : c);
		}
		return hash;
	}

	// 支持隐式转换，让调用方少写几个字
	public static implicit operator FilePath(string value) => new(value);
}
