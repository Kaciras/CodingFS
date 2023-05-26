using System;
namespace CodingFS;

[Flags]
public enum FileType
{
	SourceFile = 0,
	Dependency = 1,
	Generated = 2,
}
