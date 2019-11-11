using System;

namespace CodingFS
{
	[Flags]
	public enum RecognizeType
	{
		/// <summary>
		/// 不关心（无法识别）的文件
		/// </summary>
		NotCare = 0,

		/// <summary>
		/// 不关心该目录，但其中的文件有能识别的
		/// </summary>
		Uncertain = 1,

		/// <summary>
		/// 被忽略的文件，如构建产物等
		/// </summary>
		Ignored = 2,

		/// <summary>
		/// 依赖文件，多为IDE配置和第三方的文件
		/// </summary>
		Dependency = 4,
	}
}
