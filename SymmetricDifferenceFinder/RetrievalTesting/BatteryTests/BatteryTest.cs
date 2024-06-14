using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SymmetricDifferenceFinder.RetrievalTesting.SimpleTests;

namespace SymmetricDifferenceFinder.RetrievalTesting.BatteryTests
{
	public record class BatteryTest(double StartingMultiplier, double EndingMultiplier, double Step, int Size)
	{
		public IEnumerable<BatteryDecodingResult> Run(Func<int, DecodingResult> test, int numberOfTestsInBattery)
		{
			Stopwatch stopwatch = new Stopwatch();

			var results = new List<BatteryDecodingResult>();
			for (double multiplier = StartingMultiplier; multiplier < EndingMultiplier; multiplier += Step)
			{
				stopwatch.Start();
				int numberOfItems = (int)(multiplier * Size);

				var decodingResults = Enumerable.Range(0, numberOfTestsInBattery).AsParallel().Select(_ => stopwatch.ElapsedMilliseconds)
						.Select(t => (test(numberOfItems), stopwatch.ElapsedMilliseconds - t)).ToList();

				var decodedCorrectly = (double)decodingResults
					.Where(x => x.Item1.DecodedIncorrectly == 0).Count()
					/ (double)numberOfTestsInBattery;

				var meanAverageDecoded = decodingResults
					.Sum((x) => (x.Item1.Size - x.Item1.DecodedIncorrectly) / x.Item1.Size) / (double)numberOfTestsInBattery;

				var meanTimeElapsed = decodingResults.Average(decodingResults => decodingResults.Item2);

				var fullness = multiplier;

				stopwatch.Stop();

				yield return new BatteryDecodingResult(fullness, decodedCorrectly, meanAverageDecoded, meanTimeElapsed);
			}
		}
	}
}
