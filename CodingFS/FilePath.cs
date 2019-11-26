using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace CodingFS
{
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
			if (!(obj is FilePath path))
			{
				return false;
			}

			var thisVal = Path.TrimEndingDirectorySeparator(Value);
			var objVal = Path.TrimEndingDirectorySeparator(path.Value);

			if (thisVal.Length != objVal.Length)
			{
				return false;
			}

			for (var i = thisVal.Length - 1; i >= 0; i--)
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

		public override int GetHashCode()
		{
			var hash = 80059289;
			foreach (var c in Path.TrimEndingDirectorySeparator(Value))
			{
				var n = c == '\\' ? '/' : c;
				hash = 31 * hash + n;
			}
			return hash;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsSep(char c)
		{
			return c == '\\' || c == '/';
		}

		public static implicit operator FilePath(string value) => new FilePath(value);
	}
}
