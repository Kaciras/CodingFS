using System;
using System.Collections.Generic;

namespace CodingFS;

public sealed class FileGroup
{
	public ICollection<string> Sources { get; } = new List<string>();
	public ICollection<string> Builds { get; } = new List<string>();
	public ICollection<string> Dependencies { get; } = new List<string>();

	public ICollection<string> this[FileType type] => type switch
	{
		FileType.SourceFile => Sources,
		FileType.Dependency => Dependencies,
		FileType.Generated => Builds,
		_ => throw new ArgumentException("未处理的 FileType"),
	};
}
