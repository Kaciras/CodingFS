using System;
using System.IO;
using System.Reflection;
using System.Text;
using BenchmarkDotNet.Attributes;
using CodingFS.Helper;
using CodingFS.Workspaces;

namespace CodingFS.Benchmark;

/**
 * | Method     | Mean      | StdDev    | Median    | Ratio | RatioSD |
 * |----------- |----------:|----------:|----------:|------:|--------:|
 * | ClassImpl  | 292.68 ns | 15.868 ns | 287.51 ns |  4.45 |    0.16 |
 * | StructImpl |  70.69 ns |  0.402 ns |  70.79 ns |  1.00 |    0.00 |
 */
[ReturnValueValidator]
public class StackStringBuilderPerf
{
	const string LOCAL_CONFIG = @"C:\Users\Kaciras\AppData\Local\JetBrains\IntelliJIdea2023.1";

	readonly JetBrainsDetector detector;
	readonly string? localConfig;

	public StackStringBuilderPerf()
	{
		localConfig = LOCAL_CONFIG;
		detector = new JetBrainsDetector();
		detector.GetType()
			.GetField("localConfig", BindingFlags.Instance | BindingFlags.NonPublic)!
			.SetValue(detector, LOCAL_CONFIG);
	}

	public string? ExternalBuildSystemV1(string path)
	{
		if (localConfig == null) return null;

		path = path.Replace('\\', '/');
		var hash = Utils.JavaStringHashCode(path);

		var builder = new StringBuilder(4096);
		builder.Append(localConfig);
		builder.Append(Path.DirectorySeparatorChar);
		builder.Append("projects");
		builder.Append(Path.DirectorySeparatorChar);
		builder.Append(Path.GetFileName(path.AsSpan()));
		builder.Append('.');
		builder.Append(hash.ToString("x2"));
		builder.Append(Path.DirectorySeparatorChar);
		builder.Append("external_build_system");
		builder.Append(Path.DirectorySeparatorChar);
		builder.Append("modules");
		return builder.ToString();
	}

	[Benchmark]
	public string? ClassImpl()
	{
		return ExternalBuildSystemV1("CodingFS");
	}

	[Benchmark(Baseline = true)]
	public string? StructImpl()
	{
		return detector.ExternalBuildSystem("CodingFS");
	}
}
