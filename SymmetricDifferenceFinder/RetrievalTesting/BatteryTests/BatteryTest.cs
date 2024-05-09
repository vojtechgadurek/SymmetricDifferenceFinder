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

				var decodingResults = Enumerable.Range(0, numberOfTestsInBattery)
					.Select(_ => test(numberOfItems)).Where(x => x.DecodedIncorrectly == 0).Count();

				var decodedCorrectly = (double)decodingResults / (double)numberOfTestsInBattery;
				var fullness = multiplier;

				results.Add(new BatteryDecodingResult(fullness, decodedCorrectly));
			}
			return results;
		}
	}
}
