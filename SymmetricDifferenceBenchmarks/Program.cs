namespace SymmetricDifferenceBenchmarks
{
	using BenchmarkDotNet.Running;
	using static SymmetricDifferenceFinderBenchmarks.EncoderBenchmarks;

	public class Program
	{
		public static void Main(string[] args)
		{
			BenchmarkRunner.Run<BasicEncodingBenchmark>();
		}
	}
}