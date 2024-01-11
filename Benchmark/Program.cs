using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Running;
using CodingFS.Benchmark;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static")]

[module: SkipLocalsInit]

BenchmarkRunner.Run<UnsafeDokanPerf>();
