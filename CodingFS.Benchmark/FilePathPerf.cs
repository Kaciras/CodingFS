using BenchmarkDotNet.Attributes;

namespace CodingFS.Benchmark;

public class FilePathPerf
{
	private readonly string path1;
	private readonly string path2;

	private readonly FilePath filePath1;
	private readonly FilePath filePath2;

	public FilePathPerf()
	{
		path1 = @"D:\Coding\JavaScript\test_project\node_modules\@typescript-eslint\typescript-estree\node_modules\debug\src\";
		path2 = @"D:\Coding\JavaScript\test_project/node_modules\@typescript-eslint/typescript-estree/dist_=====padding=====\";

		filePath1 = path1;
		filePath2 = path2;
	}

	[Benchmark]
	public int HashCode() => filePath1.GetHashCode();

	[Benchmark]
	public int StringHashCode() => path1.GetHashCode();

	[Benchmark]
	public bool IsEquals() => filePath1.Equals(filePath2);

	[Benchmark]
	public bool StringEquals() => path1.Equals(path2);
}
