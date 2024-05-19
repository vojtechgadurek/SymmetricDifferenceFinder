using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SymmetricDifferenceFinder.RetrievalTesting.SimpleTests;

namespace SymmetricDifferenceFinder.RetrievalTesting.BatteryTests
{
	public record class OptimalMultiplierFinder(
	int NumberOfTestsInEachStep,
	double LowerBound,
	double UpperBound,
	double Precision,
	int numberOfBuckets,
	int numberOfRounds
	)
	{

		public IEnumerable<BatteryDecodingResult> Run(
			Func<int, DecodingResult> test,
			int numberTestsInBattery
			)
		{
			double upperBound = UpperBound;
			double lowerBound = LowerBound;
			var answer = new List<BatteryDecodingResult>();
			for (int i = 0; i < numberOfRounds; i++)
			{
				double step = (upperBound - lowerBound) / numberOfBuckets;
				var decodingResults = new BatteryTest(lowerBound, upperBound, step, NumberOfTestsInEachStep).Run(test, numberTestsInBattery);
				var bestResult = decodingResults.Where(x => x.MeanDecodedCorrectly > Precision).MaxBy(x => x.Fullness);

				if (bestResult is null)
				{
					bestResult = new BatteryDecodingResult(lowerBound, -1, 0);
				}

				answer.Add(bestResult);
				upperBound = bestResult.Fullness + step * numberOfBuckets / 4;
				lowerBound = bestResult.Fullness + step * numberOfBuckets / 4;
				continue;
			}
			return answer;
		}
	}
}
