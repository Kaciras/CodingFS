using System;

namespace CodingFS;

[Flags]
public enum RecognizeType
{
	/// <summary>
	/// Not Dependency nor Ignored.
	/// </summary>
	NotCare = 0,

	/// <summary>
	/// Temp file, build output...
	/// </summary>
	Ignored = 1,

	/// <summary>
	/// 3rd party file, config file...
	/// </summary>
	Dependency = 2,
}
