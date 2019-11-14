using System;
using System.Collections.Generic;

namespace CodingFS
{
	public sealed class FileGroup
	{
		public ICollection<string> Sources { get; } = new List<string>();
		public ICollection<string> Builds { get; } = new List<string>();
		public ICollection<string> Dependencies { get; } = new List<string>();

		public ICollection<string> this[FileType type] => type switch
		{
			FileType.Source => Sources,
			FileType.Dependency => Dependencies,
			FileType.Build => Builds,
			_ => throw new Exception("未处理的FileType"),
		};
	}
}
