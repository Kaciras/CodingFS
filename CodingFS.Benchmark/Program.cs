using System.Runtime.CompilerServices;
using System.Threading;
using BenchmarkDotNet.Running;
using CodingFS.Benchmark;

[module: SkipLocalsInit]

BenchmarkRunner.Run<FileClassifierPerf>();

//var fsw = new FSUnsafePerf();
//fsw.MountFileSystem();
//fsw.ReadDefault();
