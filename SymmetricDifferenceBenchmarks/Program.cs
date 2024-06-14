namespace SymmetricDifferenceFinderBenchmarks
{
	using BenchmarkDotNet.Running;
	using SymmetricDifferenceFinder.RetrievalTesting.BatteryTests;
	using SymmetricDifferenceFinderBenchmarks.Experiments;
	using static SymmetricDifferenceFinderBenchmarks.EncoderBenchmarks;

	public class Program
	{

		public static void Main(string[] args)
		{
			//BenchmarkRunner.Run<BasicEncodingBenchmark>();
			BenchmarkRunner.Run<FastaFileReaderBenchmark>();
		}
	}
}