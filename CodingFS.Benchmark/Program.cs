using System.Runtime.CompilerServices;
using BenchmarkDotNet.Running;
using CodingFS.Benchmark;

[module: SkipLocalsInit]

BenchmarkRunner.Run<CodingScannerPerf>();
