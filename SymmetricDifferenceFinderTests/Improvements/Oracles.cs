using SymmetricDifferenceFinder.Improvements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SymmetricDifferenceFinder.Improvements.Oracles;

namespace SymmetricDifferenceFinderTests.Improvements
{

	public class TestCanonicalOrders
	{
		[Fact]
		void TestCanonicalOrder()
		{
			CanonicalOrder a = default;
			var res = a.Other(a.Other(1));
			Assert.Equal(1UL, res);
		}
	}
}
