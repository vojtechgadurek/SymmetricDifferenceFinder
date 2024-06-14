using System;
using System.Collections.Generic;
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
			var results = new List<BatteryDecodingResult>();
			for (double multiplier = StartingMultiplier; multiplier < EndingMultiplier; multiplier += Step)
			{
				int numberOfItems = (int)(multiplier * Size);

				var decodingResults = Enumerable.Range(0, numberOfTestsInBattery).AsParallel()
						.Select(_ => test(numberOfItems)).ToList();
				;

				var decodedCorrectly = (double)decodingResults
					.Where(x => x.DecodedIncorrectly == 0).Count()
					/ (double)numberOfTestsInBattery;

				var meanAverageDecoded = decodingResults
					.Sum(x => (x.Size - x.DecodedIncorrectly) / x.Size) / (double)numberOfTestsInBattery;

				var fullness = multiplier;

				yield return new BatteryDecodingResult(fullness, decodedCorrectly, meanAverageDecoded);
			}
		}
	}
}
