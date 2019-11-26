using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace CodingFS.Benchmark
{
	public class FilePathPerf
	{
		private readonly string path;
		private readonly FilePath filePath;

		public FilePathPerf()
		{
			path = @"D:\Coding\JavaScript\test_project\node_modules\@typescript-eslint\typescript-estree\node_modules\debug\src";
			filePath = path;
		}

		[Benchmark]
		public int HashCode() => filePath.GetHashCode();

		[Benchmark]
		public int StringHashCode() => path.GetHashCode();
	}
}
