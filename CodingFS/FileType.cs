using System;
namespace CodingFS;

[Flags]
public enum FileType
{
	Source = 0,
	Dependency = 1,
	Generated = 2,
}
