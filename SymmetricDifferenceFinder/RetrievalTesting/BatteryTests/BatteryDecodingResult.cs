using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.RetrievalTesting.BatteryTests
{
	public record class BatteryDecodingResult
	(
		double Fullness,
		double MeanDecodedCorrectly,
		double MeanAverageDecoded
	);
}
