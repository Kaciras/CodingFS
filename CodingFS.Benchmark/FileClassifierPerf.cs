using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace CodingFS.Benchmark
{
	public class FileClassifierPerf
	{
		//WorkspaceFileClassifier wfs = new WorkspaceFileClassifier();

		//[Benchmark]
		//public FileType GetType()
		//{

		//}

		private readonly string value;
		private readonly int hashCode;

		public FileClassifierPerf()
		{
			value = "test_project/src/application.json";
			hashCode = value.GetHashCode();
		}

		[Benchmark]
		public bool StringEquals()
		{
			return "test_project/src/application.xaml".Equals(value);
		}

		[Benchmark]
		public bool SpanEquals()
		{
			var span = "test_project/src/application.xaml".AsSpan();
			return span.SequenceEqual(value);
		}

		[Benchmark]
		public bool EqualByHash()
		{
			return "test_project/src/application.xaml".GetHashCode() == hashCode;
		}
	}
}
