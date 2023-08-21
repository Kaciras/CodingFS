using System.IO;
using BenchmarkDotNet.Attributes;
using CodingFS.Workspaces;
using LibGit2Sharp;

namespace CodingFS.Benchmark;

/*
 * |         Method | Matches |       Mean |     Error |    StdDev |
 * |--------------- |-------- |-----------:|----------:|----------:|
 * |    CheckBefore |   False |   4.672 us | 0.0150 us | 0.0133 us |
 * | CatchException |   False |  43.338 us | 0.1874 us | 0.1753 us |
 * |    CheckBefore |    True | 321.811 us | 3.4772 us | 3.2526 us |
 * | CatchException |    True | 316.665 us | 4.7226 us | 4.4175 us |
 */
public class GitDetectorPerf
{
	const string TEMP_PROJECT = "git-perf-temp";

	readonly GitDetector currentImpl = new(default);

	DetectContxt context;

	[Params(true, false)]
	public bool Matches { get; set; }

	[GlobalSetup]
	public void SetUp()
	{
		if (Matches)
		{
			Directory.CreateDirectory(TEMP_PROJECT);
			Repository.Init(TEMP_PROJECT);
			context = new(TEMP_PROJECT, new());
		}
		else
		{
			context = new(".", new());
		}
	}

	static void NotMatchWithException(DetectContxt ctx)
	{
		try
		{
			ctx.AddWorkspace(new GitWorkspace(ctx.Path, default));
		}
		catch (RepositoryNotFoundException)
		{

		}
	}

	[Benchmark]
	public void CheckBefore() => currentImpl.Match(context);

	[Benchmark] 
	public void CatchException() => NotMatchWithException(context); 
}
