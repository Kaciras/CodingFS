using System;
using System.IO;
using System.Reflection;
using System.Text;
using BenchmarkDotNet.Attributes;
using CodingFS.Workspaces;

namespace CodingFS.Benchmark;

/**
 * |     Method |      Mean |    Error |   StdDev |
 * |----------- |----------:|---------:|---------:|
 * |  ClassImpl | 351.93 ns | 6.822 ns | 9.563 ns |
 * | StructImpl |  77.51 ns | 0.605 ns | 0.566 ns |
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

	[Benchmark]
	public string? StructImpl()
	{
		return detector.ExternalBuildSystem("CodingFS");
	}
}
