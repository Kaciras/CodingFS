using System.Runtime.CompilerServices;
using System.Threading;
using BenchmarkDotNet.Running;
using CodingFS.Benchmark;

[module: SkipLocalsInit]

BenchmarkRunner.Run<IdeaXMLPerf>();

//var x = new IdeaXMLPerf();
//x.CheckEquality();
